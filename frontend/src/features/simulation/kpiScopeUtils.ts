import type {
  CropSeasonDto,
  KpiValueAggregation,
  SimulationKpiScopeInputDto,
} from '../../api/types'

export const KPI_KIND = {
  Loyalty: 'Loyalty',
  Quality: 'Quality',
  Financial: 'Financial',
  Esg: 'Esg',
  Technologies: 'Technologies',
  Yield: 'Yield',
  Scale: 'Scale',
} as const

export type KpiScopeFormState = {
  loyalty: { cropSeasonIds: number[] }
  quality: { cropSeasonIds: number[]; valueAggregation: KpiValueAggregation }
  financial: { cropSeasonIds: number[]; valueAggregation: KpiValueAggregation }
  esg: { cropSeasonIds: number[] }
  technologies: { cropSeasonIds: number[] }
  yield: { cropSeasonIds: number[]; valueAggregation: KpiValueAggregation }
  scale: { cropSeasonIds: number[]; valueAggregation: KpiValueAggregation }
}

export function createDefaultKpiScopeForm(): KpiScopeFormState {
  return {
    loyalty: { cropSeasonIds: [] },
    quality: { cropSeasonIds: [], valueAggregation: 'Average' },
    financial: { cropSeasonIds: [], valueAggregation: 'LastActiveCropData' },
    esg: { cropSeasonIds: [] },
    technologies: { cropSeasonIds: [] },
    yield: { cropSeasonIds: [], valueAggregation: 'Average' },
    scale: { cropSeasonIds: [], valueAggregation: 'LastActiveCropData' },
  }
}

export function buildKpiScopesFromForm(form: KpiScopeFormState): SimulationKpiScopeInputDto[] {
  return [
    { kpiKind: KPI_KIND.Loyalty, cropSeasonIds: form.loyalty.cropSeasonIds },
    {
      kpiKind: KPI_KIND.Quality,
      cropSeasonIds: form.quality.cropSeasonIds,
      valueAggregation: form.quality.valueAggregation,
    },
    {
      kpiKind: KPI_KIND.Financial,
      cropSeasonIds: form.financial.cropSeasonIds,
      valueAggregation: form.financial.valueAggregation,
    },
    { kpiKind: KPI_KIND.Esg, cropSeasonIds: form.esg.cropSeasonIds },
    { kpiKind: KPI_KIND.Technologies, cropSeasonIds: form.technologies.cropSeasonIds },
    {
      kpiKind: KPI_KIND.Yield,
      cropSeasonIds: form.yield.cropSeasonIds,
      valueAggregation: form.yield.valueAggregation,
    },
    {
      kpiKind: KPI_KIND.Scale,
      cropSeasonIds: form.scale.cropSeasonIds,
      valueAggregation: form.scale.valueAggregation,
    },
  ]
}

export function isKpiScopeFormComplete(form: KpiScopeFormState): boolean {
  return (
    form.loyalty.cropSeasonIds.length > 0 &&
    form.quality.cropSeasonIds.length > 0 &&
    form.financial.cropSeasonIds.length > 0 &&
    form.esg.cropSeasonIds.length > 0 &&
    form.technologies.cropSeasonIds.length > 0 &&
    form.yield.cropSeasonIds.length > 0 &&
    form.scale.cropSeasonIds.length > 0
  )
}

function aggregationLabel(value: KpiValueAggregation | null | undefined): string {
  if (!value) return ''
  return value === 'Average' ? 'Average' : 'Last active'
}

export function formatKpiScopesSummary(
  scopes: SimulationKpiScopeInputDto[],
  seasons: CropSeasonDto[],
): string {
  const codeById = new Map(seasons.map((s) => [s.id, s.code]))
  return scopes
    .map((scope) => {
      const codes = scope.cropSeasonIds
        .map((id) => codeById.get(id) ?? String(id))
        .join(', ')
      const agg = aggregationLabel(scope.valueAggregation)
      const aggSuffix = agg ? ` (${agg})` : ''
      return `${scope.kpiKind}: ${codes}${aggSuffix}`
    })
    .join(' · ')
}

export function toggleSeasonId(ids: number[], id: number): number[] {
  return ids.includes(id)
    ? ids.filter((x) => x !== id)
    : [...ids, id].sort((a, b) => b - a)
}
