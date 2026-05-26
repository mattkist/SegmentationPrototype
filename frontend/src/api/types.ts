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
  contractKpi: FarmerContractKpiRowDto | null
  technologies: TechnologiesKpiRowDto[]
  esgIrregularities: EsgIrregularityKpiRowDto[]
}

export interface FarmerContractKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  cultureTypeCode: string
  deliveredPercentage: number
  deliveredAmountKg: number
  contractedAmountKg: number
  iqs: number
  hadNtrm: boolean
  hadQualityMixture: boolean
  selfFundingPercentage: number
  haveDebt: boolean
  yield: number
  scale: number
  reforestationPercentage: number
  nativeForestPercentage: number
  nonExclusive: boolean
}

export interface TechnologiesKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  cultureTypeCode: string
  technologyId: number
  technologyName: string
}

export interface EsgIrregularityKpiRowDto {
  farmerCode: string
  cropSeasonId: number
  cropSeasonCode: string
  cultureTypeCode: string
  irregularityTypeId: number
  irregularityTypeName: string
}

export interface TechnologyDto {
  id: number
  name: string
}

export interface IrregularityTypeDto {
  id: number
  name: string
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
  maxScore: number
  relevance: number
  seasonQuantityRanges: LoyaltySeasonQuantityRangeDto[]
  historicalVolumeRanges: LoyaltyHistoricalVolumeRangeDto[]
}

export interface SegmentationLoyaltyDetailDto extends SegmentationLoyaltyWriteDto {}

export interface QualityIqsRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  score: number
}

export interface SegmentationQualityWriteDto {
  maxScore: number
  relevance: number
  ntrmScore: number
  mixtureScore: number
  iqsRanges: QualityIqsRangeDto[]
}

export interface SegmentationQualityDetailDto extends SegmentationQualityWriteDto {}

export interface FinancialSelfFundingRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  score: number
}

export interface SegmentationFinancialWriteDto {
  maxScore: number
  relevance: number
  debtScore: number
  selfFundingRanges: FinancialSelfFundingRangeDto[]
}

export interface SegmentationFinancialDetailDto extends SegmentationFinancialWriteDto {}

export interface TechnologyScoreDto {
  technologyId: number
  score: number
}

export interface SegmentationTechnologyWriteDto {
  maxScore: number
  relevance: number
  technologyScores: TechnologyScoreDto[]
}

export interface SegmentationTechnologyDetailDto extends SegmentationTechnologyWriteDto {}

export interface EsgIrregularityScoreDto {
  irregularityTypeId: number
  score: number
}

export interface SegmentationEsgWriteDto {
  maxScore: number
  relevance: number
  reforestationScorePerPercentualPoint: number
  reforestationMaximumScore: number
  nativeForestScorePerPercentualPoint: number
  nativeForestMaximumScore: number
  irregularityScores: EsgIrregularityScoreDto[]
}

export interface SegmentationEsgDetailDto extends SegmentationEsgWriteDto {}

export interface YieldRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  score: number
}

export interface SegmentationYieldWriteDto {
  maxScore: number
  relevance: number
  ranges: YieldRangeDto[]
}

export interface SegmentationYieldDetailDto extends SegmentationYieldWriteDto {}

export interface ScaleRangeDto {
  minimum: number
  maximum: number
  cropSeasonAmount: number
  score: number
}

export interface SegmentationScaleWriteDto {
  maxScore: number
  relevance: number
  ranges: ScaleRangeDto[]
}

export interface SegmentationScaleDetailDto extends SegmentationScaleWriteDto {}

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
}

export interface SaveSegmentationConfigurationDto {
  name: string
  segments: SegmentationSegmentDto[]
  cultureTypes: CultureTypeConfigurationWriteDto[]
}

export type KpiValueAggregation = 'Average' | 'LastActiveCropData'

export interface SimulationKpiScopeInputDto {
  kpiKind: string
  cropSeasonIds: number[]
  valueAggregation?: KpiValueAggregation | null
}

export interface CreateSegmentationSimulationDto {
  segmentationConfigurationId: string
  cropSeasonId: number
  kpiScopes: SimulationKpiScopeInputDto[]
}

export interface SegmentationSimulationSummaryDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  kpiScopes: SimulationKpiScopeInputDto[]
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
  nonExclusiveFarmer: boolean
  segmentationConfigurationSegmentId: string | null
  segmentName: string | null
  isNewFarmer: boolean
}

export interface SegmentShareDto {
  segmentName: string
  farmerCount: number
  percentage: number
}

export interface CultureTypeSegmentDistributionDto {
  cultureTypeCode: string
  segments: SegmentShareDto[]
}

export interface SegmentationSimulationDetailDto {
  id: string
  segmentationConfigurationId: string
  configurationName: string
  cropSeasonId: number
  cropSeasonCode: string
  kpiScopes: SimulationKpiScopeInputDto[]
  simulationDate: string
  status: string
  farmers: SegmentationSimulationFarmerDto[]
  overallSegmentDistribution: SegmentShareDto[]
  segmentDistributionByCultureType: CultureTypeSegmentDistributionDto[]
}

export interface SegmentationManagementRowDto {
  farmerId: string
  farmerCode: string
  farmerName: string
  cultureTypeCode: string
  totalScore: number
  segmentationConfigurationSegmentId: string | null
  segmentName: string | null
  segmentationConfigurationId: string
  availableSegments: SegmentationSegmentDto[]
}

export interface SubmitSegmentationApprovalDto {
  segmentationConfigurationSegmentId: string | null
}

export interface ApiErrorBody {
  message?: string
  sumOfKpiMaxScores?: number
  maximumScore?: number
}
