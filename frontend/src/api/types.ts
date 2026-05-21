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
  yieldAndScaleScore: number
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
  cultureTypeCodes: string[]
}

export interface SegmentationSegmentDto {
  id?: string | null
  segmentName: string
  onlyExclusiveFarmer: boolean
  bankDepositDiscount: number
  tobaccoDiscount: number
}

export interface CultureTypeSegmentThresholdDto {
  segmentId?: string | null
  segmentName: string
  rangeMin: number | null
}

export interface LoyaltySeasonQuantityRangeDto {
  plantingCropSeasonAmount: number
  minimumDeliveryAmount: number
  maximumDeliveryAmount: number
  deliveryCropSeasonAmount: number
  score: number
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
  score: number
}

export interface SegmentationQualityWriteDto {
  relevance: number
  ntrmScore: number
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
  score: number
}

export interface SegmentationFinancialWriteDto {
  relevance: number
  debtScore: number
  selfFundingRanges: FinancialSelfFundingRangeDto[]
}

export interface SegmentationFinancialDetailDto extends SegmentationFinancialWriteDto {
  maxScore: number
}

export interface SegmentationTechnologyWriteDto {
  relevance: number
  hasLargeBaseRidgeWithMulchScore: number
  hasBroadGrateFurnaceScore: number
  hasTechnologyPackageAdherenceScore: number
  hasStandardBarnScore: number
}

export interface SegmentationTechnologyDetailDto extends SegmentationTechnologyWriteDto {
  maxScore: number
}

export interface SegmentationEsgWriteDto {
  relevance: number
  reforestationScorePerPercentualPoint: number
  reforestationMaximumScore: number
  nativeForestScorePerPercentualPoint: number
  nativeForestMaximumScore: number
  minorIrregularityScore: number
  majorIrregularityScore: number
}

export interface SegmentationEsgDetailDto extends SegmentationEsgWriteDto {
  maxScore: number
}

export interface YieldRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  score: number
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
  score: number
}

export interface SegmentationScaleWriteDto {
  relevance: number
  ranges: ScaleRangeDto[]
}

export interface SegmentationScaleDetailDto extends SegmentationScaleWriteDto {
  maxScore: number
}

export interface YieldAndScaleRangeDto {
  yieldAndScaleCropSeasonAmount: number
  minimumYield: number
  maximumYield: number
  minimumModule: number
  maximumModule: number
  score: number
}

export interface SegmentationYieldAndScaleWriteDto {
  relevance: number
  ranges: YieldAndScaleRangeDto[]
}

export interface SegmentationYieldAndScaleDetailDto extends SegmentationYieldAndScaleWriteDto {
  maxScore: number
}

export interface CultureTypeConfigurationDetailDto {
  id?: string | null
  cultureTypeCode: string
  maximumScore: number
  segmentThresholds: CultureTypeSegmentThresholdDto[]
  loyalty: SegmentationLoyaltyDetailDto
  quality: SegmentationQualityDetailDto
  financial: SegmentationFinancialDetailDto
  technology: SegmentationTechnologyDetailDto
  esg: SegmentationEsgDetailDto
  yield: SegmentationYieldDetailDto
  scale: SegmentationScaleDetailDto
  yieldAndScale: SegmentationYieldAndScaleDetailDto
}

export interface SegmentationConfigurationDetailDto {
  id: string
  name: string
  segments: SegmentationSegmentDto[]
  cultureTypes: CultureTypeConfigurationDetailDto[]
}

export interface CultureTypeConfigurationWriteDto {
  id?: string | null
  cultureTypeCode: string
  maximumScore: number
  segmentThresholds: CultureTypeSegmentThresholdDto[]
  loyalty: SegmentationLoyaltyWriteDto
  quality: SegmentationQualityWriteDto
  financial: SegmentationFinancialWriteDto
  technology: SegmentationTechnologyWriteDto
  esg: SegmentationEsgWriteDto
  yield: SegmentationYieldWriteDto
  scale: SegmentationScaleWriteDto
  yieldAndScale: SegmentationYieldAndScaleWriteDto
}

export interface SaveSegmentationConfigurationDto {
  name: string
  segments: SegmentationSegmentDto[]
  cultureTypes: CultureTypeConfigurationWriteDto[]
}

export interface CreateSegmentationSimulationDto {
  segmentationConfigurationId: string
  cropSeasonId: number
  scopeCropSeasonIds: number[]
}

export interface SegmentationSimulationSummaryDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  scopeCropSeasonIds: number[]
  simulationDate: string
  status: string
  farmerCount: number
}

export interface SegmentationSimulationFarmerDto {
  farmerId: string
  farmerCode: string
  farmerName: string
  cultureTypeCode: string
  totalScore: number
  loyaltyScore: number
  qualityScore: number
  financialScore: number
  technologiesScore: number
  esgScore: number
  yieldScore: number
  scaleScore: number
  yieldAndScaleScore: number
  nonExclusiveFarmer: boolean
  segmentationConfigurationSegmentId: string | null
  segmentName: string | null
}

export interface SegmentationSimulationDetailDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  scopeCropSeasonIds: number[]
  simulationDate: string
  status: string
  farmers: SegmentationSimulationFarmerDto[]
}

export interface ApiErrorBody {
  message?: string
  sumOfKpiMaxScores?: number
  maximumScore?: number
}
