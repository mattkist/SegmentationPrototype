import type {
  SaveSegmentationConfigurationDto,
  SegmentationConfigurationDetailDto,
} from '../api/types'
import { relevanceFieldToFraction } from './relevanceFraction'
import { syncRelevancesFromCaps } from './kpiRelevanceCaps'

export function detailToSaveDto(d: SegmentationConfigurationDetailDto): SaveSegmentationConfigurationDto {
  const base: SaveSegmentationConfigurationDto = {
    name: d.name,
    cultureTypeCode: d.cultureTypeCode,
    maximumScore: d.maximumScore,
    segments: d.segments.map((s) => ({
      id: s.id,
      segmentName: s.segmentName,
      rangeMin: s.rangeMin,
      onlyExclusiveFarmer: s.onlyExclusiveFarmer,
      bankDepositDiscount: s.bankDepositDiscount,
      tobaccoDiscount: s.tobaccoDiscount,
    })),
    loyalty: {
      relevance: relevanceFieldToFraction((d.loyalty as { relevance?: unknown }).relevance),
      seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((r) => ({ ...r })),
      historicalVolumeRanges: d.loyalty.historicalVolumeRanges.map((r) => ({ ...r })),
    },
    quality: {
      relevance: relevanceFieldToFraction((d.quality as { relevance?: unknown }).relevance),
      ntrmCropSeasonAmount: d.quality.ntrmCropSeasonAmount,
      ntrmCropSeasonStart: d.quality.ntrmCropSeasonStart,
      ntrmScore: d.quality.ntrmScore,
      mixtureCropSeasonAmount: d.quality.mixtureCropSeasonAmount,
      mixtureCropSeasonStart: d.quality.mixtureCropSeasonStart,
      mixtureScore: d.quality.mixtureScore,
      iqsRanges: d.quality.iqsRanges.map((r) => ({
        ...r,
        skippedCropSeasonIds: [...r.skippedCropSeasonIds],
      })),
    },
    financial: {
      relevance: relevanceFieldToFraction((d.financial as { relevance?: unknown }).relevance),
      debtCropSeason: d.financial.debtCropSeason,
      debtScore: d.financial.debtScore,
      selfFundingRanges: d.financial.selfFundingRanges.map((r) => ({
        ...r,
        skippedCropSeasonIds: [...r.skippedCropSeasonIds],
      })),
    },
    technology: {
      relevance: relevanceFieldToFraction((d.technology as { relevance?: unknown }).relevance),
      hasLargeBaseRidgeWithMulchCropSeason: d.technology.hasLargeBaseRidgeWithMulchCropSeason,
      hasLargeBaseRidgeWithMulchScore: d.technology.hasLargeBaseRidgeWithMulchScore,
      hasBroadGrateFurnaceCropSeason: d.technology.hasBroadGrateFurnaceCropSeason,
      hasBroadGrateFurnaceScore: d.technology.hasBroadGrateFurnaceScore,
      hasTechnologyPackageAdherenceCropSeason: d.technology.hasTechnologyPackageAdherenceCropSeason,
      hasTechnologyPackageAdherenceScore: d.technology.hasTechnologyPackageAdherenceScore,
    },
    esg: {
      relevance: relevanceFieldToFraction((d.esg as { relevance?: unknown }).relevance),
      reforestationCropSeason: d.esg.reforestationCropSeason,
      reforestationScorePerPercentualPoint: d.esg.reforestationScorePerPercentualPoint,
      reforestationMaximumScore: d.esg.reforestationMaximumScore,
      nativeForestCropSeason: d.esg.nativeForestCropSeason,
      nativeForestScorePerPercentualPoint: d.esg.nativeForestScorePerPercentualPoint,
      nativeForestMaximumScore: d.esg.nativeForestMaximumScore,
      minorIrregularityCropSeason: d.esg.minorIrregularityCropSeason,
      minorIrregularityScore: d.esg.minorIrregularityScore,
      majorIrregularityCropSeason: d.esg.majorIrregularityCropSeason,
      majorIrregularityScore: d.esg.majorIrregularityScore,
    },
    yield: {
      relevance: relevanceFieldToFraction((d.yield as { relevance?: unknown }).relevance),
      ranges: d.yield.ranges.map((r) => ({
        ...r,
        skippedCropSeasonIds: [...r.skippedCropSeasonIds],
      })),
    },
    scale: {
      relevance: relevanceFieldToFraction((d.scale as { relevance?: unknown }).relevance),
      ranges: d.scale.ranges.map((r) => ({
        ...r,
        skippedCropSeasonIds: [...r.skippedCropSeasonIds],
      })),
    },
  }
  return syncRelevancesFromCaps(base)
}

/** Build JSON body: POST omits segment ids; PUT keeps ids for existing rows. */
export function bodyForSave(
  dto: SaveSegmentationConfigurationDto,
  mode: 'create' | 'update',
): unknown {
  if (mode === 'create') {
    return {
      ...dto,
      segments: dto.segments.map(({ id: _id, ...s }) => s),
    }
  }
  return dto
}

/** True when `draft` differs from server `baseline` only in `name` (same rules/KPI payload). */
export function isOnlyConfigurationNameChange(
  draft: SaveSegmentationConfigurationDto,
  baseline: SaveSegmentationConfigurationDto,
): boolean {
  if (draft.name === baseline.name) return false
  return JSON.stringify({ ...draft, name: baseline.name }) === JSON.stringify(baseline)
}
