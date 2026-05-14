import type { SaveSegmentationConfigurationDto } from '../api/types'
import { deriveKpiMaxScores } from './deriveKpiMax'

export type KpiKey = 'loyalty' | 'quality' | 'financial' | 'technology' | 'esg' | 'yield' | 'scale'

export const KPI_KEYS: KpiKey[] = [
  'loyalty',
  'quality',
  'financial',
  'technology',
  'esg',
  'yield',
  'scale',
]

export function capsToRecord(c: ReturnType<typeof deriveKpiMaxScores>): Record<KpiKey, number> {
  return {
    loyalty: c.loyalty,
    quality: c.quality,
    financial: c.financial,
    technology: c.technology,
    esg: c.esg,
    yield: c.yield,
    scale: c.scale,
  }
}

/** Relevance fractions = derived cap ÷ maximumScore (or ÷ sum(caps) if caps do not match M yet). */
export function syncRelevancesFromCaps(d: SaveSegmentationConfigurationDto): SaveSegmentationConfigurationDto {
  const c = deriveKpiMaxScores(d)
  const M = d.maximumScore
  if (M <= 0 || c.sum <= 0) return d
  const denom = c.matchesMaximum ? M : c.sum
  const rf = (cap: number) => cap / denom
  return {
    ...d,
    loyalty: { ...d.loyalty, relevance: rf(c.loyalty) },
    quality: { ...d.quality, relevance: rf(c.quality) },
    financial: { ...d.financial, relevance: rf(c.financial) },
    technology: { ...d.technology, relevance: rf(c.technology) },
    esg: { ...d.esg, relevance: rf(c.esg) },
    yield: { ...d.yield, relevance: rf(c.yield) },
    scale: { ...d.scale, relevance: rf(c.scale) },
  }
}

/**
 * User sets one KPI's share of **total** maximum score to `newFraction` (0–1).
 * Integer caps T_i sum to M; others absorb the remainder proportionally to their current caps.
 * All internal scores in each block scale so the derived cap matches T_i (with rounding fix-up).
 */
export function applyRelevanceFractionEdit(
  d: SaveSegmentationConfigurationDto,
  edited: KpiKey,
  newFraction: number,
): SaveSegmentationConfigurationDto {
  const c = deriveKpiMaxScores(d)
  const M = d.maximumScore
  if (!c.matchesMaximum || M <= 0) return syncRelevancesFromCaps(d)

  const C = capsToRecord(c)
  const f = Math.min(1, Math.max(0, newFraction))
  const targets = allocateIntegerCapsFromEditedFraction(C, M, edited, f)
  let next = scaleDraftToIntegerCaps(d, C, targets)
  next = fixDerivedSumToMaximum(next, M)
  return syncRelevancesFromCaps(next)
}

function allocateIntegerCapsFromEditedFraction(
  C: Record<KpiKey, number>,
  M: number,
  edited: KpiKey,
  fEdited: number,
): Record<KpiKey, number> {
  const T = {} as Record<KpiKey, number>
  for (const k of KPI_KEYS) T[k] = 0

  const f = Math.min(1, Math.max(0, fEdited))
  let tEdited = Math.round(f * M)
  tEdited = Math.max(0, Math.min(M, tEdited))

  const R = M - tEdited
  const others = KPI_KEYS.filter((k) => k !== edited)
  const sumOthersCaps = others.reduce((s, k) => s + C[k], 0)

  T[edited] = tEdited
  if (R === 0) {
    for (const k of others) T[k] = 0
    return T
  }
  if (sumOthersCaps <= 0) {
    T[edited] = M
    for (const k of others) T[k] = 0
    return T
  }

  const parts = others.map((k) => {
    const raw = (R * C[k]) / sumOthersCaps
    return { k, b: Math.floor(raw), r: raw - Math.floor(raw) }
  })
  let used = tEdited + parts.reduce((s, p) => s + p.b, 0)
  let diff = M - used
  const sorted = [...parts].sort((a, b) => b.r - a.r)
  let i = 0
  while (diff > 0 && sorted.length > 0) {
    sorted[i % sorted.length].b++
    diff--
    i++
  }
  i = 0
  let guard = 0
  while (diff < 0 && sorted.length > 0 && guard++ < M + 50) {
    if (sorted[i % sorted.length].b > 0) {
      sorted[i % sorted.length].b--
      diff++
    }
    i++
  }

  for (const p of sorted) T[p.k] = p.b
  T[edited] = tEdited

  let s = KPI_KEYS.reduce((a, k) => a + T[k], 0)
  guard = 0
  while (s !== M && guard++ < M + 50) {
    const kMax = KPI_KEYS.reduce((best, k) => (T[k] > T[best] ? k : best), 'loyalty' as KpiKey)
    if (s < M) T[kMax]++
    else if (T[kMax] > 0) T[kMax]--
    s = KPI_KEYS.reduce((a, k) => a + T[k], 0)
  }

  return T
}

function scaleDraftToIntegerCaps(
  d: SaveSegmentationConfigurationDto,
  C: Record<KpiKey, number>,
  T: Record<KpiKey, number>,
): SaveSegmentationConfigurationDto {
  let next: SaveSegmentationConfigurationDto = { ...d, loyalty: { ...d.loyalty }, quality: { ...d.quality }, financial: { ...d.financial }, technology: { ...d.technology }, esg: { ...d.esg }, yield: { ...d.yield }, scale: { ...d.scale } }

  for (const k of KPI_KEYS) {
    const c0 = C[k]
    const t0 = T[k]
    if (c0 <= 0) {
      if (t0 > 0) next = seedMinimalKpiScores(next, k, t0)
      continue
    }
    const factor = t0 / c0
    next = scaleKpiBlock(next, k, factor)
  }

  return next
}

function seedMinimalKpiScores(d: SaveSegmentationConfigurationDto, k: KpiKey, target: number): SaveSegmentationConfigurationDto {
  const score = Math.max(0, target)
  switch (k) {
    case 'loyalty':
      return {
        ...d,
        loyalty: {
          ...d.loyalty,
          historicalVolumeRanges: [{ minimumDeliveryAmount: 0, maximumDeliveryAmount: 100, score }],
          seasonQuantityRanges: [],
        },
      }
    case 'quality':
      return {
        ...d,
        quality: {
          ...d.quality,
          iqsRanges: [
            {
              minimum: 0,
              maximum: 100,
              cropSeasonAmount: 1,
              cropSeasonStart: 2026,
              score,
              skippedCropSeasonIds: [],
            },
          ],
        },
      }
    case 'financial':
      return {
        ...d,
        financial: {
          ...d.financial,
          selfFundingRanges: [
            {
              minimum: 0,
              maximum: 100,
              cropSeasonAmount: 1,
              cropSeasonStart: 2026,
              score,
              skippedCropSeasonIds: [],
            },
          ],
        },
      }
    case 'technology':
      return {
        ...d,
        technology: {
          ...d.technology,
          hasLargeBaseRidgeWithMulchScore: score,
          hasBroadGrateFurnaceScore: 0,
          hasTechnologyPackageAdherenceScore: 0,
        },
      }
    case 'esg': {
      const half = Math.floor(score / 2)
      const rest = score - half
      return {
        ...d,
        esg: {
          ...d.esg,
          reforestationMaximumScore: half,
          nativeForestMaximumScore: rest,
          minorIrregularityScore: 0,
          majorIrregularityScore: 0,
        },
      }
    }
    case 'yield':
      return {
        ...d,
        yield: {
          ...d.yield,
          ranges: [
            {
              minimum: 0,
              maximum: 999999,
              cropSeasonAmount: 1,
              cropSeasonStart: 2026,
              score,
              skippedCropSeasonIds: [],
            },
          ],
        },
      }
    case 'scale':
      return {
        ...d,
        scale: {
          ...d.scale,
          ranges: [
            {
              minimum: 0,
              maximum: 999999,
              cropSeasonAmount: 1,
              cropSeasonStart: 2026,
              score,
              skippedCropSeasonIds: [],
            },
          ],
        },
      }
    default:
      return d
  }
}

function scaleKpiBlock(d: SaveSegmentationConfigurationDto, k: KpiKey, factor: number): SaveSegmentationConfigurationDto {
  if (!Number.isFinite(factor) || factor < 0) return d
  const r = (n: number) => Math.max(0, Math.round(n * factor))

  switch (k) {
    case 'loyalty':
      return {
        ...d,
        loyalty: {
          ...d.loyalty,
          historicalVolumeRanges: d.loyalty.historicalVolumeRanges.map((h) => ({ ...h, score: r(h.score) })),
          seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    case 'quality':
      return {
        ...d,
        quality: {
          ...d.quality,
          ntrmScore: r(d.quality.ntrmScore),
          mixtureScore: r(d.quality.mixtureScore),
          iqsRanges: d.quality.iqsRanges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    case 'financial':
      return {
        ...d,
        financial: {
          ...d.financial,
          debtScore: r(d.financial.debtScore),
          selfFundingRanges: d.financial.selfFundingRanges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    case 'technology':
      return {
        ...d,
        technology: {
          ...d.technology,
          hasLargeBaseRidgeWithMulchScore: r(d.technology.hasLargeBaseRidgeWithMulchScore),
          hasBroadGrateFurnaceScore: r(d.technology.hasBroadGrateFurnaceScore),
          hasTechnologyPackageAdherenceScore: r(d.technology.hasTechnologyPackageAdherenceScore),
        },
      }
    case 'esg':
      return {
        ...d,
        esg: {
          ...d.esg,
          reforestationScorePerPercentualPoint: Math.max(0, Math.round(d.esg.reforestationScorePerPercentualPoint * factor * 1000) / 1000),
          reforestationMaximumScore: r(d.esg.reforestationMaximumScore),
          nativeForestScorePerPercentualPoint: Math.max(0, Math.round(d.esg.nativeForestScorePerPercentualPoint * factor * 1000) / 1000),
          nativeForestMaximumScore: r(d.esg.nativeForestMaximumScore),
          minorIrregularityScore: r(d.esg.minorIrregularityScore),
          majorIrregularityScore: r(d.esg.majorIrregularityScore),
        },
      }
    case 'yield':
      return {
        ...d,
        yield: {
          ...d.yield,
          ranges: d.yield.ranges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    case 'scale':
      return {
        ...d,
        scale: {
          ...d.scale,
          ranges: d.scale.ranges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    default:
      return d
  }
}

/** Adjust a few scores until derived sum === M (caps validation). */
function fixDerivedSumToMaximum(d: SaveSegmentationConfigurationDto, M: number): SaveSegmentationConfigurationDto {
  let cur = d
  let prevSum = -999999
  for (let iter = 0; iter < 5000; iter++) {
    const s = deriveKpiMaxScores(cur).sum
    if (s === M) return cur
    if (s === prevSum) return cur
    prevSum = s
    const delta = M - s
    const nudged = delta > 0 ? nudgeSumUp(cur) : nudgeSumDown(cur)
    if (deriveKpiMaxScores(nudged).sum === s) return cur
    cur = nudged
  }
  return cur
}

function nudgeSumUp(d: SaveSegmentationConfigurationDto): SaveSegmentationConfigurationDto {
  const loc = firstScoreLocation(d)
  if (!loc) return d
  return applyDeltaAt(d, loc, 1)
}

function nudgeSumDown(d: SaveSegmentationConfigurationDto): SaveSegmentationConfigurationDto {
  const loc = lastPositiveScoreLocation(d)
  if (!loc) return d
  return applyDeltaAt(d, loc, -1)
}

type ScoreLoc =
  | { k: 'loyalty'; kind: 'hist' | 'season'; i: number }
  | { k: 'quality'; kind: 'iqs' | 'ntrm' | 'mix'; i?: number }
  | { k: 'financial'; kind: 'sf' | 'debt'; i?: number }
  | { k: 'technology'; slot: 'm' | 'f' | 'p' }
  | { k: 'esg'; field: string }
  | { k: 'yield' | 'scale'; i: number }

function firstScoreLocation(d: SaveSegmentationConfigurationDto): ScoreLoc | null {
  if (d.loyalty.historicalVolumeRanges.length) return { k: 'loyalty', kind: 'hist', i: 0 }
  if (d.loyalty.seasonQuantityRanges.length) return { k: 'loyalty', kind: 'season', i: 0 }
  if (d.quality.iqsRanges.length) return { k: 'quality', kind: 'iqs', i: 0 }
  if (d.financial.selfFundingRanges.length) return { k: 'financial', kind: 'sf', i: 0 }
  if (d.yield.ranges.length) return { k: 'yield', i: 0 }
  if (d.scale.ranges.length) return { k: 'scale', i: 0 }
  if (d.esg.reforestationMaximumScore > 0 || d.esg.nativeForestMaximumScore > 0)
    return { k: 'esg', field: 'refMax' }
  return { k: 'technology', slot: 'm' }
}

function lastPositiveScoreLocation(d: SaveSegmentationConfigurationDto): ScoreLoc | null {
  const tryScale = () => {
    for (let i = d.scale.ranges.length - 1; i >= 0; i--) {
      if (d.scale.ranges[i].score > 0) return { k: 'scale' as const, i }
    }
    return null
  }
  const tryYield = () => {
    for (let i = d.yield.ranges.length - 1; i >= 0; i--) {
      if (d.yield.ranges[i].score > 0) return { k: 'yield' as const, i }
    }
    return null
  }
  const tryHist = () => {
    for (let i = d.loyalty.historicalVolumeRanges.length - 1; i >= 0; i--) {
      if (d.loyalty.historicalVolumeRanges[i].score > 0) return { k: 'loyalty' as const, kind: 'hist' as const, i }
    }
    return null
  }
  return tryYield() ?? tryScale() ?? tryHist() ?? firstScoreLocation(d)
}

function applyDeltaAt(d: SaveSegmentationConfigurationDto, loc: ScoreLoc, delta: number): SaveSegmentationConfigurationDto {
  switch (loc.k) {
    case 'loyalty': {
      if (loc.kind === 'hist') {
        const arr = [...d.loyalty.historicalVolumeRanges]
        arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
        return { ...d, loyalty: { ...d.loyalty, historicalVolumeRanges: arr } }
      }
      const arr = [...d.loyalty.seasonQuantityRanges]
      arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
      return { ...d, loyalty: { ...d.loyalty, seasonQuantityRanges: arr } }
    }
    case 'quality': {
      if (loc.kind === 'iqs' && loc.i !== undefined) {
        const arr = [...d.quality.iqsRanges]
        arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
        return { ...d, quality: { ...d.quality, iqsRanges: arr } }
      }
      if (loc.kind === 'ntrm')
        return { ...d, quality: { ...d.quality, ntrmScore: Math.max(0, d.quality.ntrmScore + delta) } }
      return { ...d, quality: { ...d.quality, mixtureScore: Math.max(0, d.quality.mixtureScore + delta) } }
    }
    case 'financial': {
      if (loc.kind === 'debt')
        return { ...d, financial: { ...d.financial, debtScore: Math.max(0, d.financial.debtScore + delta) } }
      const arr = [...d.financial.selfFundingRanges]
      const i = loc.i ?? 0
      arr[i] = { ...arr[i], score: Math.max(0, arr[i].score + delta) }
      return { ...d, financial: { ...d.financial, selfFundingRanges: arr } }
    }
    case 'technology': {
      const t = { ...d.technology }
      if (loc.slot === 'm') t.hasLargeBaseRidgeWithMulchScore = Math.max(0, t.hasLargeBaseRidgeWithMulchScore + delta)
      else if (loc.slot === 'f') t.hasBroadGrateFurnaceScore = Math.max(0, t.hasBroadGrateFurnaceScore + delta)
      else t.hasTechnologyPackageAdherenceScore = Math.max(0, t.hasTechnologyPackageAdherenceScore + delta)
      return { ...d, technology: t }
    }
    case 'esg': {
      const e = { ...d.esg }
      if (loc.field === 'refMax') e.reforestationMaximumScore = Math.max(0, e.reforestationMaximumScore + delta)
      else e.nativeForestMaximumScore = Math.max(0, e.nativeForestMaximumScore + delta)
      return { ...d, esg: e }
    }
    case 'yield': {
      const arr = [...d.yield.ranges]
      arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
      return { ...d, yield: { ...d.yield, ranges: arr } }
    }
    case 'scale': {
      const arr = [...d.scale.ranges]
      arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
      return { ...d, scale: { ...d.scale, ranges: arr } }
    }
    default:
      return d
  }
}
