import type { CultureTypeConfigurationWriteDto } from '../api/types'
import { KPI_KEYS, type KpiKey } from './kpiRelevanceCaps'

export function roundTo2(n: number): number {
  return Math.round(n * 100) / 100
}

export function formatRelevancePercent(fraction: number): string {
  if (!Number.isFinite(fraction)) return '0.00'
  return roundTo2(fraction * 100).toFixed(2)
}

export function getKpiRelevanceFraction(d: CultureTypeConfigurationWriteDto, k: KpiKey): number {
  switch (k) {
    case 'loyalty':
      return d.loyalty.relevance
    case 'quality':
      return d.quality.relevance
    case 'financial':
      return d.financial.relevance
    case 'technology':
      return d.technology.relevance
    case 'esg':
      return d.esg.relevance
    case 'yield':
      return d.yield.relevance
    case 'scale':
      return d.scale.relevance
    case 'yieldAndScale':
      return d.yieldAndScale.relevance
  }
}

/** Total relevance % from stored fractions (2 decimal places for display only). */
export function relevanceSumPercent(d: CultureTypeConfigurationWriteDto): number {
  const rawPercent =
    KPI_KEYS.reduce((s, k) => s + getKpiRelevanceFraction(d, k), 0) * 100
  return roundTo2(rawPercent)
}

/** True when stored fractions sum to 100% (tolerant of float noise; not per-field rounding). */
export function isRelevanceSumExactly100(d: CultureTypeConfigurationWriteDto): boolean {
  const rawFraction = KPI_KEYS.reduce((s, k) => s + getKpiRelevanceFraction(d, k), 0)
  if (Math.abs(rawFraction - 1) < 0.0001) return true
  return relevanceSumPercent(d) === 100
}
