import type { SaveSegmentationConfigurationDto } from '../api/types'
import { syncSegmentThresholdsForHeader } from './cultureTypeDraft'

function compareDesc(a: number, b: number): number {
  return b - a
}

export function sortSegmentsByTobaccoDiscount<T extends { tobaccoDiscount: number }>(segments: T[]): T[] {
  return [...segments].sort((a, b) => compareDesc(a.tobaccoDiscount, b.tobaccoDiscount))
}

export function sortByScoreDesc<T extends { score: number }>(ranges: T[]): T[] {
  return [...ranges].sort((a, b) => compareDesc(a.score, b.score))
}

export function sortSeasonQuantityRanges<
  T extends { score: number; deliveryCropSeasonAmount: number },
>(ranges: T[]): T[] {
  return [...ranges].sort((a, b) => {
    const byScore = compareDesc(a.score, b.score)
    if (byScore !== 0) return byScore
    return compareDesc(a.deliveryCropSeasonAmount, b.deliveryCropSeasonAmount)
  })
}

/** Applies UI sort order for segments, thresholds, and score-based ranges. */
export function normalizeConfigurationOrder(
  draft: SaveSegmentationConfigurationDto,
): SaveSegmentationConfigurationDto {
  const segments = sortSegmentsByTobaccoDiscount(draft.segments)
  const cultureTypes = draft.cultureTypes.map((ct) => ({
    ...ct,
    loyalty: {
      ...ct.loyalty,
      seasonQuantityRanges: sortSeasonQuantityRanges(ct.loyalty.seasonQuantityRanges),
    },
    quality: {
      ...ct.quality,
      iqsRanges: sortByScoreDesc(ct.quality.iqsRanges),
    },
    financial: {
      ...ct.financial,
      selfFundingRanges: sortByScoreDesc(ct.financial.selfFundingRanges),
    },
    yield: {
      ...ct.yield,
      ranges: sortByScoreDesc(ct.yield.ranges),
    },
    scale: {
      ...ct.scale,
      ranges: sortByScoreDesc(ct.scale.ranges),
    },
    yieldAndScale: {
      ...ct.yieldAndScale,
      ranges: sortByScoreDesc(ct.yieldAndScale.ranges),
    },
  }))

  return syncSegmentThresholdsForHeader({ ...draft, segments, cultureTypes })
}
