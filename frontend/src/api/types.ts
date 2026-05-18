export interface CropSeasonDto {
  id: number
  code: string
}

export interface CultureTypeDto {
  code: string
  name: string
}

export interface FarmerListItemDto {
  farmerId: string
  farmerCode: string
  farmerName: string
  nonExclusiveFarmer: boolean
  totalScore: number | null
  rank: number | null
  segmentName: string | null
  segmentationConfigurationSegmentId: string | null
}

export interface OfficialSegmentationDto {
  totalScore: number
  rank: number
  segmentName: string | null
  segmentationConfigurationSegmentId: string | null
  loyaltyScore: number
  qualityScore: number
  financialScore: number
  technologiesScore: number
  esgScore: number
  yieldScore: number
  scaleScore: number
}

export interface FarmerDetailDto {
  farmerId: string
  farmerCode: string
  farmerName: string
  nonExclusiveFarmer: boolean
  cropSeasonId: number
  cropSeasonCode: string
  officialSegmentation: OfficialSegmentationDto | null
  kpis: FarmerKpisForSeasonDto
}

export interface FarmerKpisForSeasonDto {
  loyalty: LoyaltyKpiRowDto | null
  quality: QualityKpiRowDto | null
  financial: FinancialKpiRowDto | null
  yield: YieldKpiRowDto | null
  scale: ScaleKpiRowDto | null
  technologies: TechnologiesKpiRowDto | null
  esg: EsgKpiRowDto | null
}

export interface LoyaltyKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  deliveredPercentage: number
}

export interface QualityKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  iqs: number
  hadNtrm: boolean
  hadQualityMixture: boolean
}

export interface FinancialKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  selfFundingPercentage: number
  haveDebt: boolean
}

export interface YieldKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  yield: number
}

export interface ScaleKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  scale: number
}

export interface TechnologiesKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  hasLargeBaseRidgeWithMulch: boolean
  hasBroadGrateFurnace: boolean
  hasTechnologyPackageAdherence: boolean
}

export interface EsgKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  reforestationPercentage: number
  nativeForestPercentage: number
  hasMinorIrregularity: boolean
  hasMajorIrregularity: boolean
}

export interface KpiImportResultDto {
  totalRows: number
  insertedRows: number
  updatedRows: number
  errors: { rowNumber: number; message: string }[]
}

export interface SegmentationConfigurationSummaryDto {
  id: string
  name: string
  cultureTypeCode: string
  maximumScore: number
}

export interface SegmentationSegmentDto {
  id?: string | null
  segmentName: string
  rangeMin: number | null
  onlyExclusiveFarmer: boolean
  bankDepositDiscount: number
  tobaccoDiscount: number
}

export interface LoyaltySeasonQuantityRangeDto {
  plantingCropSeasonAmount: number
  cropSeasonStart: number
  minimumDeliveryAmount: number
  maximumDeliveryAmount: number
  deliveryCropSeasonAmount: number
  score: number
  skippedCropSeasonIds: number[]
}

export interface LoyaltyHistoricalVolumeRangeDto {
  minimumDeliveryAmount: number
  maximumDeliveryAmount: number
  score: number
}

export interface SegmentationLoyaltyWriteDto {
  relevance: number
  seasonQuantityRanges: LoyaltySeasonQuantityRangeDto[]
  historicalVolumeRanges: LoyaltyHistoricalVolumeRangeDto[]
}

export interface SegmentationLoyaltyDetailDto extends SegmentationLoyaltyWriteDto {
  maxScore: number
}

export interface QualityIqsRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  cropSeasonStart: number
  score: number
  skippedCropSeasonIds: number[]
}

export interface SegmentationQualityWriteDto {
  relevance: number
  ntrmCropSeasonAmount: number
  ntrmCropSeasonStart: number
  ntrmScore: number
  mixtureCropSeasonAmount: number
  mixtureCropSeasonStart: number
  mixtureScore: number
  iqsRanges: QualityIqsRangeDto[]
}

export interface SegmentationQualityDetailDto extends SegmentationQualityWriteDto {
  maxScore: number
}

export interface FinancialSelfFundingRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  cropSeasonStart: number
  score: number
  skippedCropSeasonIds: number[]
}

export interface SegmentationFinancialWriteDto {
  relevance: number
  debtCropSeason: number
  debtScore: number
  selfFundingRanges: FinancialSelfFundingRangeDto[]
}

export interface SegmentationFinancialDetailDto extends SegmentationFinancialWriteDto {
  maxScore: number
}

export interface SegmentationTechnologyWriteDto {
  relevance: number
  hasLargeBaseRidgeWithMulchCropSeason: number
  hasLargeBaseRidgeWithMulchScore: number
  hasBroadGrateFurnaceCropSeason: number
  hasBroadGrateFurnaceScore: number
  hasTechnologyPackageAdherenceCropSeason: number
  hasTechnologyPackageAdherenceScore: number
}

export interface SegmentationTechnologyDetailDto extends SegmentationTechnologyWriteDto {
  maxScore: number
}

export interface SegmentationEsgWriteDto {
  relevance: number
  reforestationCropSeason: number
  reforestationScorePerPercentualPoint: number
  reforestationMaximumScore: number
  nativeForestCropSeason: number
  nativeForestScorePerPercentualPoint: number
  nativeForestMaximumScore: number
  minorIrregularityCropSeason: number
  minorIrregularityScore: number
  majorIrregularityCropSeason: number
  majorIrregularityScore: number
}

export interface SegmentationEsgDetailDto extends SegmentationEsgWriteDto {
  maxScore: number
}

export interface YieldRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  cropSeasonStart: number
  score: number
  skippedCropSeasonIds: number[]
}

export interface SegmentationYieldWriteDto {
  relevance: number
  ranges: YieldRangeDto[]
}

export interface SegmentationYieldDetailDto extends SegmentationYieldWriteDto {
  maxScore: number
}

export interface ScaleRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  cropSeasonStart: number
  score: number
  skippedCropSeasonIds: number[]
}

export interface SegmentationScaleWriteDto {
  relevance: number
  ranges: ScaleRangeDto[]
}

export interface SegmentationScaleDetailDto extends SegmentationScaleWriteDto {
  maxScore: number
}

export interface SegmentationConfigurationDetailDto {
  id: string
  name: string
  cultureTypeCode: string
  maximumScore: number
  segments: SegmentationSegmentDto[]
  loyalty: SegmentationLoyaltyDetailDto
  quality: SegmentationQualityDetailDto
  financial: SegmentationFinancialDetailDto
  technology: SegmentationTechnologyDetailDto
  esg: SegmentationEsgDetailDto
  yield: SegmentationYieldDetailDto
  scale: SegmentationScaleDetailDto
}

export interface SaveSegmentationConfigurationDto {
  name: string
  cultureTypeCode: string
  maximumScore: number
  segments: SegmentationSegmentDto[]
  loyalty: SegmentationLoyaltyWriteDto
  quality: SegmentationQualityWriteDto
  financial: SegmentationFinancialWriteDto
  technology: SegmentationTechnologyWriteDto
  esg: SegmentationEsgWriteDto
  yield: SegmentationYieldWriteDto
  scale: SegmentationScaleWriteDto
}

export interface CreateSegmentationSimulationDto {
  segmentationConfigurationId: string
  cropSeasonId: number
}

export interface SegmentationSimulationSummaryDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  simulationDate: string
  status: string
  farmerCount: number
}

export interface SegmentationSimulationFarmerDto {
  farmerId: string
  farmerCode: string
  farmerName: string
  totalScore: number
  loyaltyScore: number
  qualityScore: number
  financialScore: number
  technologiesScore: number
  esgScore: number
  yieldScore: number
  scaleScore: number
  nonExclusiveFarmer: boolean
  segmentationConfigurationSegmentId: string | null
  segmentName: string | null
  rank: number
}

export interface SegmentationSimulationDetailDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  simulationDate: string
  status: string
  farmers: SegmentationSimulationFarmerDto[]
}

export interface ApiErrorBody {
  message?: string
  sumOfKpiMaxScores?: number
  maximumScore?: number
}
