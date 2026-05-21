import type { CultureTypeConfigurationWriteDto } from '../api/types'

function maxPositiveOrZero(scores: number[]): number {
  const pos = scores.filter((s) => s > 0)
  return pos.length === 0 ? 0 : Math.max(...pos)
}

function sumPositiveScores(...scores: number[]): number {
  return scores.filter((s) => s > 0).reduce((x, y) => x + y, 0)
}

/** Mirrors backend SegmentationConfigurationKpiMaxScores for live UI feedback. */
export function deriveKpiMaxScores(block: CultureTypeConfigurationWriteDto) {
  const loyalty =
    maxPositiveOrZero(block.loyalty.seasonQuantityRanges.map((r) => r.score)) +
    maxPositiveOrZero(block.loyalty.historicalVolumeRanges.map((r) => r.score))

  const quality =
    block.quality.iqsRanges.length === 0
      ? 0
      : Math.max(...block.quality.iqsRanges.map((r) => r.score))

  const financial =
    block.financial.selfFundingRanges.length === 0
      ? 0
      : Math.max(...block.financial.selfFundingRanges.map((r) => r.score))

  const technology = sumPositiveScores(
    block.technology.hasLargeBaseRidgeWithMulchScore,
    block.technology.hasBroadGrateFurnaceScore,
    block.technology.hasTechnologyPackageAdherenceScore,
    block.technology.hasStandardBarnScore,
  )

  const esg = block.esg.reforestationMaximumScore + block.esg.nativeForestMaximumScore

  const yieldScore =
    block.yield.ranges.length === 0 ? 0 : Math.max(...block.yield.ranges.map((r) => r.score))

  const scaleScore =
    block.scale.ranges.length === 0 ? 0 : Math.max(...block.scale.ranges.map((r) => r.score))

  const yieldAndScale =
    block.yieldAndScale.ranges.length === 0
      ? 0
      : maxPositiveOrZero(block.yieldAndScale.ranges.map((r) => r.score))

  const sum =
    loyalty + quality + financial + technology + esg + yieldScore + scaleScore + yieldAndScale

  return {
    loyalty,
    quality,
    financial,
    technology,
    esg,
    yield: yieldScore,
    scale: scaleScore,
    yieldAndScale,
    sum,
    matchesMaximum: sum === block.maximumScore,
  }
}
