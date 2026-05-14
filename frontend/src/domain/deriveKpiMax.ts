import type { SaveSegmentationConfigurationDto } from '../api/types'

function maxPositiveOrZero(scores: number[]): number {
  const pos = scores.filter((s) => s > 0)
  return pos.length === 0 ? 0 : Math.max(...pos)
}

function sumPositiveScores(a: number, b: number, c: number): number {
  return [a, b, c].filter((s) => s > 0).reduce((x, y) => x + y, 0)
}

/** Mirrors backend SegmentationConfigurationKpiMaxScores for live UI feedback. */
export function deriveKpiMaxScores(dto: SaveSegmentationConfigurationDto) {
  const loyalty =
    maxPositiveOrZero(dto.loyalty.seasonQuantityRanges.map((r) => r.score)) +
    maxPositiveOrZero(dto.loyalty.historicalVolumeRanges.map((r) => r.score))

  const quality =
    dto.quality.iqsRanges.length === 0
      ? 0
      : Math.max(...dto.quality.iqsRanges.map((r) => r.score))

  const financial =
    dto.financial.selfFundingRanges.length === 0
      ? 0
      : Math.max(...dto.financial.selfFundingRanges.map((r) => r.score))

  const technology = sumPositiveScores(
    dto.technology.hasLargeBaseRidgeWithMulchScore,
    dto.technology.hasBroadGrateFurnaceScore,
    dto.technology.hasTechnologyPackageAdherenceScore,
  )

  const esg = dto.esg.reforestationMaximumScore + dto.esg.nativeForestMaximumScore

  const yieldScore =
    dto.yield.ranges.length === 0 ? 0 : Math.max(...dto.yield.ranges.map((r) => r.score))

  const scaleScore =
    dto.scale.ranges.length === 0 ? 0 : Math.max(...dto.scale.ranges.map((r) => r.score))

  const sum =
    loyalty + quality + financial + technology + esg + yieldScore + scaleScore

  return {
    loyalty,
    quality,
    financial,
    technology,
    esg,
    yield: yieldScore,
    scale: scaleScore,
    sum,
    matchesMaximum: sum === dto.maximumScore,
  }
}
