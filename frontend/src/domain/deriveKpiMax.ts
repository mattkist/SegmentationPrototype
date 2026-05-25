import type { CultureTypeConfigurationWriteDto } from '../api/types'

function maxPositiveOrZero(scores: number[]): number {
  const pos = scores.filter((s) => s > 0)
  return pos.length === 0 ? 0 : Math.max(...pos)
}

function sumPositiveScores(...scores: number[]): number {
  return scores.filter((s) => s > 0).reduce((x, y) => x + y, 0)
}

export type KpiMaxKey =
  | 'loyalty'
  | 'quality'
  | 'financial'
  | 'technology'
  | 'esg'
  | 'yield'
  | 'scale'
  | 'yieldAndScale'

/** Mirrors backend SegmentationConfigurationKpiMaxScores derived caps (rules only). */
export function deriveKpiMaxScores(block: CultureTypeConfigurationWriteDto) {
  const loyaltyDerived =
    maxPositiveOrZero(block.loyalty.seasonQuantityRanges.map((r) => r.score)) +
    maxPositiveOrZero(block.loyalty.historicalVolumeRanges.map((r) => r.score))

  const qualityDerived =
    block.quality.iqsRanges.length === 0
      ? 0
      : Math.max(...block.quality.iqsRanges.map((r) => r.score))

  const financialDerived =
    block.financial.selfFundingRanges.length === 0
      ? 0
      : Math.max(...block.financial.selfFundingRanges.map((r) => r.score))

  const technologyDerived = sumPositiveScores(
    ...block.technology.technologyScores.map((t) => t.score),
  )

  const esgDerived =
    block.esg.reforestationMaximumScore +
    block.esg.nativeForestMaximumScore +
    sumPositiveScores(...block.esg.irregularityScores.map((i) => i.score))

  const yieldDerived =
    block.yield.ranges.length === 0 ? 0 : Math.max(...block.yield.ranges.map((r) => r.score))

  const scaleDerived =
    block.scale.ranges.length === 0 ? 0 : Math.max(...block.scale.ranges.map((r) => r.score))

  const yieldAndScaleDerived =
    block.yieldAndScale.ranges.length === 0
      ? 0
      : maxPositiveOrZero(block.yieldAndScale.ranges.map((r) => r.score))

  const derived = {
    loyalty: loyaltyDerived,
    quality: qualityDerived,
    financial: financialDerived,
    technology: technologyDerived,
    esg: esgDerived,
    yield: yieldDerived,
    scale: scaleDerived,
    yieldAndScale: yieldAndScaleDerived,
  }

  const configured = {
    loyalty: block.loyalty.maxScore,
    quality: block.quality.maxScore,
    financial: block.financial.maxScore,
    technology: block.technology.maxScore,
    esg: block.esg.maxScore,
    yield: block.yield.maxScore,
    scale: block.scale.maxScore,
    yieldAndScale: block.yieldAndScale.maxScore,
  }

  const sumConfigured = Object.values(configured).reduce((a, b) => a + b, 0)
  const sumDerived = Object.values(derived).reduce((a, b) => a + b, 0)

  const kpiMatches = (key: KpiMaxKey) => derived[key] === configured[key]

  return {
    ...derived,
    configured,
    sumConfigured,
    sumDerived,
    matchesMaximum: sumConfigured === block.maximumScore,
    allKpiRulesMatchConfigured: (Object.keys(derived) as KpiMaxKey[]).every(kpiMatches),
    kpiMatches,
  }
}
