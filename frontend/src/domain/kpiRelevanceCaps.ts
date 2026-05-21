import type { CultureTypeConfigurationWriteDto } from '../api/types'
import { deriveKpiMaxScores } from './deriveKpiMax'
import { isRelevanceSumExactly100 } from './relevanceDisplay'

export type KpiKey =
  | 'loyalty'
  | 'quality'
  | 'financial'
  | 'technology'
  | 'esg'
  | 'yield'
  | 'scale'
  | 'yieldAndScale'

export const KPI_KEYS: KpiKey[] = [
  'loyalty',
  'quality',
  'financial',
  'technology',
  'esg',
  'yield',
  'scale',
  'yieldAndScale',
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
    yieldAndScale: c.yieldAndScale,
  }
}

/** Relevance fractions = derived cap ÷ maximumScore (or ÷ sum(caps) if caps do not match M yet). */
export function syncRelevancesFromCaps(d: CultureTypeConfigurationWriteDto): CultureTypeConfigurationWriteDto {
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
    yieldAndScale: { ...d.yieldAndScale, relevance: rf(c.yieldAndScale) },
  }
}

export function getRelevanceFractions(d: CultureTypeConfigurationWriteDto): Record<KpiKey, number> {
  return {
    loyalty: d.loyalty.relevance,
    quality: d.quality.relevance,
    financial: d.financial.relevance,
    technology: d.technology.relevance,
    esg: d.esg.relevance,
    yield: d.yield.relevance,
    scale: d.scale.relevance,
    yieldAndScale: d.yieldAndScale.relevance,
  }
}

export function setKpiRelevance(
  d: CultureTypeConfigurationWriteDto,
  k: KpiKey,
  fraction: number,
): CultureTypeConfigurationWriteDto {
  const f = Math.min(1, Math.max(0, fraction))
  switch (k) {
    case 'loyalty':
      return { ...d, loyalty: { ...d.loyalty, relevance: f } }
    case 'quality':
      return { ...d, quality: { ...d.quality, relevance: f } }
    case 'financial':
      return { ...d, financial: { ...d.financial, relevance: f } }
    case 'technology':
      return { ...d, technology: { ...d.technology, relevance: f } }
    case 'esg':
      return { ...d, esg: { ...d.esg, relevance: f } }
    case 'yield':
      return { ...d, yield: { ...d.yield, relevance: f } }
    case 'scale':
      return { ...d, scale: { ...d.scale, relevance: f } }
    case 'yieldAndScale':
      return { ...d, yieldAndScale: { ...d.yieldAndScale, relevance: f } }
  }
}

/**
 * Scales all KPI rule scores so derived caps match relevance shares and sum exactly to M.
 * Call only when relevance % sum is exactly 100.00 (see relevanceDisplay).
 */
export function recalculatePointsFromRelevances(
  d: CultureTypeConfigurationWriteDto,
): CultureTypeConfigurationWriteDto {
  const M = d.maximumScore
  if (M <= 0 || !isRelevanceSumExactly100(d)) return d

  const fractions = getRelevanceFractions(d)
  const targets = allocateIntegerCapsFromFractions(fractions, M)
  const C = capsToRecord(deriveKpiMaxScores(d))
  let next = scaleDraftToIntegerCaps(d, C, targets)
  next = fixDerivedSumToMaximum(next, M)
  return next
}

/** Integer caps T_i sum to M from relevance fractions (largest-remainder). */
export function allocateIntegerCapsFromFractions(
  fractions: Record<KpiKey, number>,
  M: number,
): Record<KpiKey, number> {
  const T = {} as Record<KpiKey, number>
  for (const k of KPI_KEYS) T[k] = 0
  if (M <= 0) return T

  const weights = KPI_KEYS.map((k) => Math.max(0, fractions[k]))
  const weightSum = weights.reduce((s, w) => s + w, 0)
  const parts = KPI_KEYS.map((k, i) => {
    const w = weightSum > 0 ? weights[i] / weightSum : 1 / KPI_KEYS.length
    const exact = w * M
    const floor = Math.floor(exact)
    return { k, floor, remainder: exact - floor }
  })

  for (const p of parts) T[p.k] = p.floor
  let diff = M - parts.reduce((s, p) => s + p.floor, 0)
  const byRemainder = [...parts].sort((a, b) => b.remainder - a.remainder)
  let i = 0
  while (diff > 0 && byRemainder.length > 0) {
    T[byRemainder[i % byRemainder.length].k]++
    diff--
    i++
  }

  let total = KPI_KEYS.reduce((s, k) => s + T[k], 0)
  let guard = 0
  while (total > M && guard++ < M + 50) {
    const k = KPI_KEYS.reduce((best, key) => (T[key] > T[best] ? key : best), KPI_KEYS[0])
    if (T[k] <= 0) break
    T[k]--
    total--
  }
  guard = 0
  while (total < M && guard++ < M + 50) {
    T[byRemainder[0]?.k ?? KPI_KEYS[0]]++
    total++
  }

  return T
}

function scaleDraftToIntegerCaps(
  d: CultureTypeConfigurationWriteDto,
  C: Record<KpiKey, number>,
  T: Record<KpiKey, number>,
): CultureTypeConfigurationWriteDto {
  let next: CultureTypeConfigurationWriteDto = {
    ...d,
    loyalty: { ...d.loyalty },
    quality: { ...d.quality },
    financial: { ...d.financial },
    technology: { ...d.technology },
    esg: { ...d.esg },
    yield: { ...d.yield },
    scale: { ...d.scale },
    yieldAndScale: { ...d.yieldAndScale },
  }

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

function seedMinimalKpiScores(d: CultureTypeConfigurationWriteDto, k: KpiKey, target: number): CultureTypeConfigurationWriteDto {
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
              score,
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
              score,
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
              score,
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
              score,
            },
          ],
        },
      }
    case 'yieldAndScale':
      return {
        ...d,
        yieldAndScale: {
          ...d.yieldAndScale,
          ranges: [
            {
              yieldAndScaleCropSeasonAmount: 1,
              minimumYield: 0,
              maximumYield: 999999,
              minimumModule: 0,
              maximumModule: 999999,
              score,
            },
          ],
        },
      }
    default:
      return d
  }
}

function scaleKpiBlock(d: CultureTypeConfigurationWriteDto, k: KpiKey, factor: number): CultureTypeConfigurationWriteDto {
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
    case 'yieldAndScale':
      return {
        ...d,
        yieldAndScale: {
          ...d.yieldAndScale,
          ranges: d.yieldAndScale.ranges.map((x) => ({ ...x, score: r(x.score) })),
        },
      }
    default:
      return d
  }
}

/** Adjust scores until derived sum === M (never leaves sum > M). */
function fixDerivedSumToMaximum(d: CultureTypeConfigurationWriteDto, M: number): CultureTypeConfigurationWriteDto {
  let cur = d
  for (let iter = 0; iter < M * 8 + 100; iter++) {
    const s = deriveKpiMaxScores(cur).sum
    if (s === M) return cur
    if (s > M) {
      const down = nudgeSumDown(cur)
      if (deriveKpiMaxScores(down).sum < s) cur = down
      else return trimSumToMaximum(cur, M)
    } else {
      const up = nudgeSumUp(cur)
      if (deriveKpiMaxScores(up).sum > s) cur = up
      else return cur
    }
  }
  return trimSumToMaximum(cur, M)
}

function trimSumToMaximum(d: CultureTypeConfigurationWriteDto, M: number): CultureTypeConfigurationWriteDto {
  let cur = d
  for (let iter = 0; iter < M * 8 + 100; iter++) {
    const s = deriveKpiMaxScores(cur).sum
    if (s <= M) return cur
    const down = nudgeSumDown(cur)
    const next = deriveKpiMaxScores(down).sum
    if (next >= s) return cur
    cur = down
  }
  return cur
}

function nudgeSumUp(d: CultureTypeConfigurationWriteDto): CultureTypeConfigurationWriteDto {
  const loc = firstScoreLocation(d)
  if (!loc) return d
  return applyDeltaAt(d, loc, 1)
}

function nudgeSumDown(d: CultureTypeConfigurationWriteDto): CultureTypeConfigurationWriteDto {
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
  | { k: 'yield' | 'scale' | 'yieldAndScale'; i: number }

function firstScoreLocation(d: CultureTypeConfigurationWriteDto): ScoreLoc | null {
  if (d.loyalty.historicalVolumeRanges.length) return { k: 'loyalty', kind: 'hist', i: 0 }
  if (d.loyalty.seasonQuantityRanges.length) return { k: 'loyalty', kind: 'season', i: 0 }
  if (d.quality.iqsRanges.length) return { k: 'quality', kind: 'iqs', i: 0 }
  if (d.financial.selfFundingRanges.length) return { k: 'financial', kind: 'sf', i: 0 }
  if (d.yieldAndScale.ranges.length) return { k: 'yieldAndScale', i: 0 }
  if (d.yield.ranges.length) return { k: 'yield', i: 0 }
  if (d.scale.ranges.length) return { k: 'scale', i: 0 }
  if (d.esg.reforestationMaximumScore > 0 || d.esg.nativeForestMaximumScore > 0)
    return { k: 'esg', field: 'refMax' }
  return { k: 'technology', slot: 'm' }
}

function lastPositiveScoreLocation(d: CultureTypeConfigurationWriteDto): ScoreLoc | null {
  const tryYieldAndScale = () => {
    for (let i = d.yieldAndScale.ranges.length - 1; i >= 0; i--) {
      if (d.yieldAndScale.ranges[i].score > 0) return { k: 'yieldAndScale' as const, i }
    }
    return null
  }
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
  return tryYieldAndScale() ?? tryYield() ?? tryScale() ?? tryHist() ?? firstScoreLocation(d)
}

function applyDeltaAt(d: CultureTypeConfigurationWriteDto, loc: ScoreLoc, delta: number): CultureTypeConfigurationWriteDto {
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
    case 'yieldAndScale': {
      const arr = [...d.yieldAndScale.ranges]
      arr[loc.i] = { ...arr[loc.i], score: Math.max(0, arr[loc.i].score + delta) }
      return { ...d, yieldAndScale: { ...d.yieldAndScale, ranges: arr } }
    }
    default:
      return d
  }
}
