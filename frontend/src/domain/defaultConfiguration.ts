import type {
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
} from '../api/types'
import { syncRelevancesFromCaps } from './kpiRelevanceCaps'

export function createCultureTypeBlock(code: string): CultureTypeConfigurationWriteDto {
  return syncRelevancesFromCaps({
    cultureTypeCode: code,
    maximumScore: 100,
    segmentThresholds: [
      { segmentName: 'Diamond', rangeMin: 75 },
      { segmentName: 'Gold', rangeMin: 45 },
      { segmentName: 'Standard', rangeMin: null },
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
      ntrmScore: 0,
      mixtureScore: 0,
      iqsRanges: [
        { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 20 },
      ],
    },
    financial: {
      relevance: 0.15,
      debtScore: 0,
      selfFundingRanges: [
        { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 15 },
      ],
    },
    technology: {
      relevance: 0.12,
      hasLargeBaseRidgeWithMulchScore: 5,
      hasBroadGrateFurnaceScore: 5,
      hasTechnologyPackageAdherenceScore: 5,
      hasStandardBarnScore: 0,
    },
    esg: {
      relevance: 0.13,
      reforestationScorePerPercentualPoint: 1,
      reforestationMaximumScore: 10,
      nativeForestScorePerPercentualPoint: 1,
      nativeForestMaximumScore: 10,
      minorIrregularityScore: 0,
      majorIrregularityScore: 0,
    },
    yield: {
      relevance: 0.15,
      ranges: [{ minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 10 }],
    },
    scale: {
      relevance: 0.15,
      ranges: [{ minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 10 }],
    },
    yieldAndScale: {
      relevance: 0,
      ranges: [],
    },
  })
}

/** Balanced template with header segments and one FCV culture-type block. */
export function createDefaultConfiguration(): SaveSegmentationConfigurationDto {
  return {
    name: 'New configuration',
    segments: [
      {
        segmentName: 'Diamond',
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
      {
        segmentName: 'Gold',
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
      {
        segmentName: 'Standard',
        onlyExclusiveFarmer: false,
        bankDepositDiscount: 0,
        tobaccoDiscount: 0,
      },
    ],
    cultureTypes: [createCultureTypeBlock('FCV')],
  }
}
