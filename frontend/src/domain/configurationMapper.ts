import type {
  CultureTypeConfigurationDetailDto,
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
  SegmentationConfigurationDetailDto,
  SegmentationTechnologyDetailDto,
  SegmentationEsgDetailDto,
} from '../api/types'
import { normalizeConfigurationOrder } from './configurationSort'
import { relevanceFieldToFraction } from './relevanceFraction'

function mapTechnologyWrite(t: SegmentationTechnologyDetailDto) {
  const legacy = t as SegmentationTechnologyDetailDto & {
    hasLargeBaseRidgeWithMulchScore?: number
    hasBroadGrateFurnaceScore?: number
    hasTechnologyPackageAdherenceScore?: number
    hasStandardBarnScore?: number
  }
  const technologyScores =
    t.technologyScores?.length > 0
      ? t.technologyScores.map((s) => ({ ...s }))
      : [
          { technologyId: 1, score: legacy.hasLargeBaseRidgeWithMulchScore ?? 0 },
          { technologyId: 2, score: legacy.hasBroadGrateFurnaceScore ?? 0 },
          { technologyId: 3, score: legacy.hasTechnologyPackageAdherenceScore ?? 0 },
          { technologyId: 4, score: legacy.hasStandardBarnScore ?? 0 },
        ].filter((s) => s.score !== 0 || [1, 2, 3, 4].includes(s.technologyId))

  return {
    maxScore: t.maxScore,
    relevance: relevanceFieldToFraction((t as { relevance?: unknown }).relevance),
    technologyScores,
  }
}

function mapEsgWrite(e: SegmentationEsgDetailDto) {
  const legacy = e as SegmentationEsgDetailDto & {
    minorIrregularityScore?: number
    majorIrregularityScore?: number
  }
  const irregularityScores =
    e.irregularityScores?.length > 0
      ? e.irregularityScores.map((s) => ({ ...s }))
      : [
          { irregularityTypeId: 1, score: legacy.minorIrregularityScore ?? 0 },
          { irregularityTypeId: 2, score: legacy.majorIrregularityScore ?? 0 },
        ].filter((s) => s.score !== 0)

  return {
    maxScore: e.maxScore,
    relevance: relevanceFieldToFraction((e as { relevance?: unknown }).relevance),
    reforestationScorePerPercentualPoint: e.reforestationScorePerPercentualPoint,
    reforestationMaximumScore: e.reforestationMaximumScore,
    nativeForestScorePerPercentualPoint: e.nativeForestScorePerPercentualPoint,
    nativeForestMaximumScore: e.nativeForestMaximumScore,
    irregularityScores,
  }
}

function mapCultureTypeWrite(ct: CultureTypeConfigurationDetailDto): CultureTypeConfigurationWriteDto {
  return {
    id: ct.id,
    cultureTypeCode: ct.cultureTypeCode,
    maximumScore: ct.maximumScore,
    segmentThresholds: ct.segmentThresholds.map((t) => ({ ...t })),
    loyalty: {
      maxScore: ct.loyalty.maxScore,
      relevance: relevanceFieldToFraction((ct.loyalty as { relevance?: unknown }).relevance),
      seasonQuantityRanges: ct.loyalty.seasonQuantityRanges.map((r) => ({ ...r })),
      historicalVolumeRanges: ct.loyalty.historicalVolumeRanges.map((r) => ({ ...r })),
    },
    quality: {
      maxScore: ct.quality.maxScore,
      relevance: relevanceFieldToFraction((ct.quality as { relevance?: unknown }).relevance),
      ntrmScore: ct.quality.ntrmScore,
      mixtureScore: ct.quality.mixtureScore,
      iqsRanges: ct.quality.iqsRanges.map((r) => ({ ...r })),
    },
    financial: {
      maxScore: ct.financial.maxScore,
      relevance: relevanceFieldToFraction((ct.financial as { relevance?: unknown }).relevance),
      debtScore: ct.financial.debtScore,
      selfFundingRanges: ct.financial.selfFundingRanges.map((r) => ({ ...r })),
    },
    technology: mapTechnologyWrite(ct.technology),
    esg: mapEsgWrite(ct.esg),
    yield: {
      maxScore: ct.yield.maxScore,
      relevance: relevanceFieldToFraction((ct.yield as { relevance?: unknown }).relevance),
      ranges: ct.yield.ranges.map((r) => ({ ...r })),
    },
    scale: {
      maxScore: ct.scale.maxScore,
      relevance: relevanceFieldToFraction((ct.scale as { relevance?: unknown }).relevance),
      ranges: ct.scale.ranges.map((r) => ({ ...r })),
    },
    yieldAndScale: {
      maxScore: ct.yieldAndScale.maxScore,
      relevance: relevanceFieldToFraction((ct.yieldAndScale as { relevance?: unknown }).relevance),
      ranges: ct.yieldAndScale.ranges.map((r) => ({ ...r })),
    },
  }
}

export function detailToSaveDto(d: SegmentationConfigurationDetailDto): SaveSegmentationConfigurationDto {
  return normalizeConfigurationOrder({
    name: d.name,
    segments: d.segments.map((s) => ({
      id: s.id,
      segmentName: s.segmentName,
      onlyExclusiveFarmer: s.onlyExclusiveFarmer,
      bankDepositDiscount: s.bankDepositDiscount,
      tobaccoDiscount: s.tobaccoDiscount,
    })),
    cultureTypes: d.cultureTypes.map(mapCultureTypeWrite),
  })
}

export function bodyForSave(
  dto: SaveSegmentationConfigurationDto,
  mode: 'create' | 'update',
): unknown {
  if (mode === 'create') {
    return {
      ...dto,
      segments: dto.segments.map(({ id: _id, ...s }) => s),
      cultureTypes: dto.cultureTypes.map(({ id: _id, ...ct }) => ct),
    }
  }
  return dto
}

export function isOnlyConfigurationNameChange(
  draft: SaveSegmentationConfigurationDto,
  baseline: SaveSegmentationConfigurationDto,
): boolean {
  if (draft.name === baseline.name) return false
  return JSON.stringify({ ...draft, name: baseline.name }) === JSON.stringify(baseline)
}
