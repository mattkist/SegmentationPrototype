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
      maxScore: 0,
      relevance: 0,
      ranges: [],
    },
    scale: {
      maxScore: 0,
      relevance: 0,
      ranges: [],
    },
    yieldAndScale: {
      maxScore: 20,
      relevance: 0.2,
      ranges: [
        {
          yieldAndScaleCropSeasonAmount: 1,
          minimumYield: 0,
          maximumYield: 999999,
          minimumModule: 0,
          maximumModule: 999999,
          score: 10,
        },
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
