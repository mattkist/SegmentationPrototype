import type { ReactNode } from 'react'
import type { CropSeasonDto, KpiValueAggregation } from '../../api/types'
import { toggleSeasonId, type KpiScopeFormState } from './kpiScopeUtils'
import { cn } from '../../lib/cn'

const AGGREGATION_OPTIONS: { value: KpiValueAggregation; label: string }[] = [
  { value: 'Average', label: 'Consider Average' },
  { value: 'LastActiveCropData', label: 'Consider Last Active Crop Data' },
]

function SeasonCheckboxes({
  seasons,
  selected,
  onChange,
}: {
  seasons: CropSeasonDto[]
  selected: number[]
  onChange: (ids: number[]) => void
}) {
  return (
    <div className="flex flex-wrap gap-1.5">
      {seasons.map((s) => (
        <label
          key={s.id}
          className="flex cursor-pointer items-center gap-1.5 rounded-md border border-black/10 bg-white px-2 py-1 text-xs"
        >
          <input
            type="checkbox"
            className="shrink-0"
            checked={selected.includes(s.id)}
            onChange={() => onChange(toggleSeasonId(selected, s.id))}
          />
          {s.code}
        </label>
      ))}
    </div>
  )
}

function AggregationRadios({
  name,
  prefix,
  value,
  onChange,
  note,
}: {
  name: string
  prefix: string
  value: KpiValueAggregation
  onChange: (v: KpiValueAggregation) => void
  note?: string
}) {
  return (
    <div className="space-y-1">
      <div className="flex flex-wrap items-center gap-x-4 gap-y-1 text-sm">
        <span className="shrink-0 font-medium text-ink">{prefix}</span>
        {AGGREGATION_OPTIONS.map((opt) => (
          <label key={opt.value} className="flex cursor-pointer items-center gap-1.5 whitespace-nowrap">
            <input
              type="radio"
              name={name}
              checked={value === opt.value}
              onChange={() => onChange(opt.value)}
            />
            {opt.label}
          </label>
        ))}
      </div>
      {note && <p className="text-xs leading-snug text-ink-muted">{note}</p>}
    </div>
  )
}

function KpiScopeRow({
  kpiLabel,
  seasonsCell,
  logicCell,
}: {
  kpiLabel: string
  seasonsCell: ReactNode
  logicCell: ReactNode
}) {
  return (
    <tr className="align-top border-t border-black/5 first:border-t-0">
      <td className="w-[7.5rem] whitespace-nowrap py-3 pr-4 text-sm font-semibold text-ink">
        {kpiLabel}
      </td>
      <td className="min-w-[12rem] py-3 pr-6">{seasonsCell}</td>
      <td className="py-3 text-sm text-ink-muted">{logicCell}</td>
    </tr>
  )
}

export function KpiScopeFormSection({
  seasons,
  form,
  onChange,
}: {
  seasons: CropSeasonDto[]
  form: KpiScopeFormState
  onChange: (next: KpiScopeFormState) => void
}) {
  return (
    <div className="mt-3 overflow-x-auto rounded-lg border border-black/5">
      <table className="w-full min-w-[640px] text-left">
        <thead>
          <tr className="bg-surface-muted/60 text-xs font-semibold uppercase tracking-wide text-ink-faint">
            <th className="px-4 py-2.5">KPI</th>
            <th className="px-4 py-2.5">Crop seasons</th>
            <th className="px-4 py-2.5">Logic</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-black/5 bg-white/50">
          <KpiScopeRow
            kpiLabel="Loyalty"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.loyalty.cropSeasonIds}
                onChange={(ids) => onChange({ ...form, loyalty: { cropSeasonIds: ids } })}
              />
            }
            logicCell={
              <span>Loyalty considers individually all the selected crop seasons.</span>
            }
          />

          <KpiScopeRow
            kpiLabel="Quality"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.quality.cropSeasonIds}
                onChange={(ids) =>
                  onChange({ ...form, quality: { ...form.quality, cropSeasonIds: ids } })
                }
              />
            }
            logicCell={
              <AggregationRadios
                name="quality-aggregation"
                prefix="IQS:"
                value={form.quality.valueAggregation}
                onChange={(valueAggregation) =>
                  onChange({ ...form, quality: { ...form.quality, valueAggregation } })
                }
                note="NTRM and Quality Mixture are always considered based on Last Active Crop Data."
              />
            }
          />

          <KpiScopeRow
            kpiLabel="Financials"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.financial.cropSeasonIds}
                onChange={(ids) =>
                  onChange({ ...form, financial: { ...form.financial, cropSeasonIds: ids } })
                }
              />
            }
            logicCell={
              <AggregationRadios
                name="financial-aggregation"
                prefix="Self Funding:"
                value={form.financial.valueAggregation}
                onChange={(valueAggregation) =>
                  onChange({ ...form, financial: { ...form.financial, valueAggregation } })
                }
                note="Debts on last season are always considered based on Last Active Crop Data."
              />
            }
          />

          <KpiScopeRow
            kpiLabel="ESG"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.esg.cropSeasonIds}
                onChange={(ids) => onChange({ ...form, esg: { cropSeasonIds: ids } })}
              />
            }
            logicCell={<span>Always consider Last Active Crop Data.</span>}
          />

          <KpiScopeRow
            kpiLabel="Technologies"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.technologies.cropSeasonIds}
                onChange={(ids) => onChange({ ...form, technologies: { cropSeasonIds: ids } })}
              />
            }
            logicCell={<span>Always consider Last Active Crop Data.</span>}
          />

          <KpiScopeRow
            kpiLabel="Yield"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.yield.cropSeasonIds}
                onChange={(ids) => onChange({ ...form, yield: { ...form.yield, cropSeasonIds: ids } })}
              />
            }
            logicCell={
              <AggregationRadios
                name="yield-aggregation"
                prefix="Yield:"
                value={form.yield.valueAggregation}
                onChange={(valueAggregation) =>
                  onChange({ ...form, yield: { ...form.yield, valueAggregation } })
                }
              />
            }
          />

          <KpiScopeRow
            kpiLabel="Scale"
            seasonsCell={
              <SeasonCheckboxes
                seasons={seasons}
                selected={form.scale.cropSeasonIds}
                onChange={(ids) => onChange({ ...form, scale: { ...form.scale, cropSeasonIds: ids } })}
              />
            }
            logicCell={
              <AggregationRadios
                name="scale-aggregation"
                prefix="Scale:"
                value={form.scale.valueAggregation}
                onChange={(valueAggregation) =>
                  onChange({ ...form, scale: { ...form.scale, valueAggregation } })
                }
              />
            }
          />
        </tbody>
      </table>
    </div>
  )
}

export function KpiScopesReadOnlyList({
  scopes,
  seasons,
  className,
}: {
  scopes: { kpiKind: string; cropSeasonIds: number[]; valueAggregation?: string | null }[]
  seasons: CropSeasonDto[]
  className?: string
}) {
  const codeById = new Map(seasons.map((s) => [s.id, s.code]))
  return (
    <ul className={cn('space-y-1 text-sm text-ink-muted', className)}>
      {scopes.map((scope) => {
        const codes = scope.cropSeasonIds.map((id) => codeById.get(id) ?? id).join(', ')
        const agg =
          scope.valueAggregation === 'Average'
            ? ' (Consider Average)'
            : scope.valueAggregation === 'LastActiveCropData'
              ? ' (Consider Last Active Crop Data)'
              : ''
        return (
          <li key={scope.kpiKind}>
            <span className="font-medium text-ink">{scope.kpiKind}</span>: {codes}
            {agg}
          </li>
        )
      })}
    </ul>
  )
}
