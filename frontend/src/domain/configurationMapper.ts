import type {
  CultureTypeConfigurationDetailDto,
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
  SegmentationConfigurationDetailDto,
} from '../api/types'
import { normalizeConfigurationOrder } from './configurationSort'
import { relevanceFieldToFraction } from './relevanceFraction'
function mapCultureTypeWrite(ct: CultureTypeConfigurationDetailDto): CultureTypeConfigurationWriteDto {
  return {
    id: ct.id,
    cultureTypeCode: ct.cultureTypeCode,
    maximumScore: ct.maximumScore,
    segmentThresholds: ct.segmentThresholds.map((t) => ({ ...t })),
    loyalty: {
      relevance: relevanceFieldToFraction((ct.loyalty as { relevance?: unknown }).relevance),
      seasonQuantityRanges: ct.loyalty.seasonQuantityRanges.map((r) => ({ ...r })),
      historicalVolumeRanges: ct.loyalty.historicalVolumeRanges.map((r) => ({ ...r })),
    },
    quality: {
      relevance: relevanceFieldToFraction((ct.quality as { relevance?: unknown }).relevance),
      ntrmScore: ct.quality.ntrmScore,
      mixtureScore: ct.quality.mixtureScore,
      iqsRanges: ct.quality.iqsRanges.map((r) => ({ ...r })),
    },
    financial: {
      relevance: relevanceFieldToFraction((ct.financial as { relevance?: unknown }).relevance),
      debtScore: ct.financial.debtScore,
      selfFundingRanges: ct.financial.selfFundingRanges.map((r) => ({ ...r })),
    },
    technology: {
      relevance: relevanceFieldToFraction((ct.technology as { relevance?: unknown }).relevance),
      hasLargeBaseRidgeWithMulchScore: ct.technology.hasLargeBaseRidgeWithMulchScore,
      hasBroadGrateFurnaceScore: ct.technology.hasBroadGrateFurnaceScore,
      hasTechnologyPackageAdherenceScore: ct.technology.hasTechnologyPackageAdherenceScore,
      hasStandardBarnScore: ct.technology.hasStandardBarnScore,
    },
    esg: {
      relevance: relevanceFieldToFraction((ct.esg as { relevance?: unknown }).relevance),
      reforestationScorePerPercentualPoint: ct.esg.reforestationScorePerPercentualPoint,
      reforestationMaximumScore: ct.esg.reforestationMaximumScore,
      nativeForestScorePerPercentualPoint: ct.esg.nativeForestScorePerPercentualPoint,
      nativeForestMaximumScore: ct.esg.nativeForestMaximumScore,
      minorIrregularityScore: ct.esg.minorIrregularityScore,
      majorIrregularityScore: ct.esg.majorIrregularityScore,
    },
    yield: {
      relevance: relevanceFieldToFraction((ct.yield as { relevance?: unknown }).relevance),
      ranges: ct.yield.ranges.map((r) => ({ ...r })),
    },
    scale: {
      relevance: relevanceFieldToFraction((ct.scale as { relevance?: unknown }).relevance),
      ranges: ct.scale.ranges.map((r) => ({ ...r })),
    },
    yieldAndScale: {
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
