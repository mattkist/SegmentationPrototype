import type {
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
} from '../api/types'

export function createCultureTypeBlock(code: string): CultureTypeConfigurationWriteDto {
  return {
    cultureTypeCode: code,
    maximumScore: 100,
    segmentThresholds: [
      { segmentName: 'Diamond', rangeMin: 75 },
      { segmentName: 'Gold', rangeMin: 45 },
      { segmentName: 'Standard', rangeMin: null },
    ],
    loyalty: {
      maxScore: 10,
      relevance: 0.1,
      seasonQuantityRanges: [],
      historicalVolumeRanges: [
        { minimumDeliveryAmount: 0, maximumDeliveryAmount: 100, score: 10 },
      ],
    },
    quality: {
      maxScore: 20,
      relevance: 0.2,
      ntrmScore: 0,
      mixtureScore: 0,
      iqsRanges: [
        { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 20 },
      ],
    },
    financial: {
      maxScore: 15,
      relevance: 0.15,
      debtScore: 0,
      selfFundingRanges: [
        { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 15 },
      ],
    },
    technology: {
      maxScore: 15,
      relevance: 0.15,
      technologyScores: [
        { technologyId: 1, score: 5 },
        { technologyId: 2, score: 5 },
        { technologyId: 3, score: 5 },
      ],
    },
    esg: {
      maxScore: 20,
      relevance: 0.2,
      reforestationScorePerPercentualPoint: 1,
      reforestationMaximumScore: 10,
      nativeForestScorePerPercentualPoint: 1,
      nativeForestMaximumScore: 10,
      irregularityScores: [],
    },
    yield: {
      maxScore: 10,
      relevance: 0.1,
      ranges: [
        { minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 10 },
      ],
    },
    scale: {
      maxScore: 10,
      relevance: 0.1,
      ranges: [
        { minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 10 },
      ],
    },
  }
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
