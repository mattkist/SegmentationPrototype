import type { SaveSegmentationConfigurationDto } from '../api/types'
import { syncRelevancesFromCaps } from './kpiRelevanceCaps'

/** Balanced template: derived KPI caps sum to 100 (matches maximumScore). */
export function createDefaultConfiguration(): SaveSegmentationConfigurationDto {
  return syncRelevancesFromCaps({
    name: 'New configuration',
    maximumScore: 100,
    segments: [
      {
        id: undefined,
        segmentName: 'Diamond',
        rangeMin: 75,
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
      {
        id: undefined,
        segmentName: 'Gold',
        rangeMin: 45,
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
      {
        id: undefined,
        segmentName: 'Standard',
        rangeMin: null,
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
    ],
    loyalty: {
      relevance: 0.15,
      seasonQuantityRanges: [],
      historicalVolumeRanges: [
        { minimumDeliveryAmount: 0, maximumDeliveryAmount: 100, score: 10 },
      ],
    },
    quality: {
      relevance: 0.15,
      ntrmCropSeasonAmount: 1,
      ntrmCropSeasonStart: 2026,
      ntrmScore: 0,
      mixtureCropSeasonAmount: 1,
      mixtureCropSeasonStart: 2026,
      mixtureScore: 0,
      iqsRanges: [
        {
          minimum: 0,
          maximum: 100,
          cropSeasonAmount: 1,
          cropSeasonStart: 2026,
          score: 20,
          skippedCropSeasonIds: [],
        },
      ],
    },
    financial: {
      relevance: 0.15,
      debtCropSeason: 2026,
      debtScore: 0,
      selfFundingRanges: [
        {
          minimum: 0,
          maximum: 100,
          cropSeasonAmount: 1,
          cropSeasonStart: 2026,
          score: 15,
          skippedCropSeasonIds: [],
        },
      ],
    },
    technology: {
      relevance: 0.12,
      hasLargeBaseRidgeWithMulchCropSeason: 2024,
      hasLargeBaseRidgeWithMulchScore: 5,
      hasBroadGrateFurnaceCropSeason: 2024,
      hasBroadGrateFurnaceScore: 5,
      hasTechnologyPackageAdherenceCropSeason: 2024,
      hasTechnologyPackageAdherenceScore: 5,
    },
    esg: {
      relevance: 0.13,
      reforestationCropSeason: 2024,
      reforestationScorePerPercentualPoint: 1,
      reforestationMaximumScore: 10,
      nativeForestCropSeason: 2024,
      nativeForestScorePerPercentualPoint: 1,
      nativeForestMaximumScore: 10,
      minorIrregularityCropSeason: 2026,
      minorIrregularityScore: 0,
      majorIrregularityCropSeason: 2026,
      majorIrregularityScore: 0,
    },
    yield: {
      relevance: 0.15,
      ranges: [
        {
          minimum: 0,
          maximum: 999999,
          cropSeasonAmount: 1,
          cropSeasonStart: 2026,
          score: 10,
          skippedCropSeasonIds: [],
        },
      ],
    },
    scale: {
      relevance: 0.15,
      ranges: [
        {
          minimum: 0,
          maximum: 999999,
          cropSeasonAmount: 1,
          cropSeasonStart: 2026,
          score: 10,
          skippedCropSeasonIds: [],
        },
      ],
    },
  })
}
