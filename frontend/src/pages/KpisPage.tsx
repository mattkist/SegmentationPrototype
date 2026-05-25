import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import * as Tabs from '@radix-ui/react-tabs'
import { useState } from 'react'
import { apiGet, apiPostFormImport, ApiRequestError } from '../api/client'
import type {
  EsgIrregularityKpiRowDto,
  EsgKpiRowDto,
  FinancialKpiRowDto,
  KpiImportResultDto,
  LoyaltyKpiRowDto,
  QualityKpiRowDto,
  TechnologiesKpiRowDto,
  YieldAndScaleKpiRowDto,
} from '../api/types'
import { useCropSeason } from '../context/CropSeasonContext'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'
import { cn } from '../lib/cn'

const tabs = [
  { id: 'loyalty', label: 'Loyalty', hint: hints.csvLoyalty },
  { id: 'quality', label: 'Quality', hint: hints.csvQuality },
  { id: 'financial', label: 'Financial', hint: hints.csvFinancial },
  { id: 'yield-and-scale', label: 'Yield & Scale', hint: hints.csvYieldAndScale },
  { id: 'technologies', label: 'Technologies', hint: hints.csvTechnologies },
  { id: 'esg', label: 'ESG', hint: hints.csvEsg },
  { id: 'esg-irregularities', label: 'ESG irregularities', hint: hints.csvEsgIrregularities },
] as const

type TabId = (typeof tabs)[number]['id']

export function KpisPage() {
  const { seasonId } = useCropSeason()
  const [tab, setTab] = useState<TabId>('loyalty')
  const [importMsg, setImportMsg] = useState<string | null>(null)
  const qc = useQueryClient()

  const loyalty = useQuery({
    queryKey: ['kpi', 'loyalty', seasonId],
    enabled: seasonId !== null && tab === 'loyalty',
    queryFn: () => apiGet<LoyaltyKpiRowDto[]>(`/api/Kpis/loyalty?cropSeasonId=${seasonId}`),
  })

  const quality = useQuery({
    queryKey: ['kpi', 'quality', seasonId],
    enabled: seasonId !== null && tab === 'quality',
    queryFn: () => apiGet<QualityKpiRowDto[]>(`/api/Kpis/quality?cropSeasonId=${seasonId}`),
  })

  const financial = useQuery({
    queryKey: ['kpi', 'financial', seasonId],
    enabled: seasonId !== null && tab === 'financial',
    queryFn: () => apiGet<FinancialKpiRowDto[]>(`/api/Kpis/financial?cropSeasonId=${seasonId}`),
  })

  const yieldAndScale = useQuery({
    queryKey: ['kpi', 'yield-and-scale', seasonId],
    enabled: seasonId !== null && tab === 'yield-and-scale',
    queryFn: () =>
      apiGet<YieldAndScaleKpiRowDto[]>(`/api/Kpis/yield-and-scale?cropSeasonId=${seasonId}`),
  })

  const technologies = useQuery({
    queryKey: ['kpi', 'technologies', seasonId],
    enabled: seasonId !== null && tab === 'technologies',
    queryFn: () =>
      apiGet<TechnologiesKpiRowDto[]>(`/api/Kpis/technologies?cropSeasonId=${seasonId}`),
  })

  const esg = useQuery({
    queryKey: ['kpi', 'esg', seasonId],
    enabled: seasonId !== null && tab === 'esg',
    queryFn: () => apiGet<EsgKpiRowDto[]>(`/api/Kpis/esg?cropSeasonId=${seasonId}`),
  })

  const esgIrregularities = useQuery({
    queryKey: ['kpi', 'esg-irregularities', seasonId],
    enabled: seasonId !== null && tab === 'esg-irregularities',
    queryFn: () =>
      apiGet<EsgIrregularityKpiRowDto[]>(
        `/api/Kpis/esg-irregularities?cropSeasonId=${seasonId}`,
      ),
  })

  const importMutation = useMutation({
    mutationFn: async ({
      file,
      path,
      kpiKey,
    }: {
      file: File
      path: string
      kpiKey: TabId
    }) => {
      const res = await apiPostFormImport(path, file)
      return { result: res as KpiImportResultDto, kpiKey }
    },
    onSuccess: ({ result, kpiKey }) => {
      void qc.invalidateQueries({ queryKey: ['kpi', kpiKey, seasonId] })
      setImportMsg(
        `Imported: ${result.insertedRows} inserted, ${result.updatedRows} updated, ${result.errors.length} errors.`,
      )
    },
    onError: (e) => {
      setImportMsg(e instanceof ApiRequestError ? e.message : 'Import failed.')
    },
  })

  if (seasonId === null) return <p className="text-ink-muted">Loading seasons…</p>

  const activeHint = tabs.find((t) => t.id === tab)?.hint ?? ''

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="font-display text-2xl font-bold text-ink">KPI data</h1>
        <Hint content={hints.kpisOverview} />
      </div>

      <Tabs.Root value={tab} onValueChange={(v) => setTab(v as TabId)}>
        <Tabs.List className="flex flex-wrap gap-1 rounded-xl border border-black/5 bg-surface-card p-1 shadow-sm">
          {tabs.map((t) => (
            <Tabs.Trigger
              key={t.id}
              value={t.id}
              className={cn(
                'rounded-lg px-3 py-2 text-sm font-medium transition',
                'data-[state=active]:bg-leaf data-[state=active]:text-white',
                'data-[state=inactive]:text-ink-muted data-[state=inactive]:hover:bg-surface-muted',
              )}
            >
              {t.label}
            </Tabs.Trigger>
          ))}
        </Tabs.List>

        <div className="mt-4 flex flex-wrap items-center justify-between gap-4 rounded-xl border border-black/5 bg-surface-card p-4 shadow-card">
          <div className="flex items-center gap-2 text-sm text-ink-muted">
            <span>CSV columns for this tab:</span>
            <Hint content={activeHint} />
          </div>
          <label className="flex cursor-pointer items-center gap-2 rounded-lg border border-dashed border-leaf/40 bg-leaf-soft/40 px-4 py-2 text-sm font-medium text-leaf hover:bg-leaf-soft">
            <input
              type="file"
              accept=".csv,text/csv"
              className="sr-only"
              disabled={importMutation.isPending}
              onChange={(e) => {
                const f = e.target.files?.[0]
                e.target.value = ''
                if (!f) return
                setImportMsg(null)
                importMutation.mutate({
                  file: f,
                  path: `/api/Kpis/${tab}/import`,
                  kpiKey: tab,
                })
              }}
            />
            {importMutation.isPending ? 'Importing…' : 'Import CSV'}
          </label>
        </div>
        {importMsg && (
          <p className="mt-2 text-sm text-ink-muted" role="status">
            {importMsg}
          </p>
        )}

        <Tabs.Content value="loyalty" className="mt-4 outline-none">
          <KpiTable rows={loyalty.data} loading={loyalty.isLoading} error={loyalty.error} />
        </Tabs.Content>
        <Tabs.Content value="quality" className="mt-4 outline-none">
          <KpiTable rows={quality.data} loading={quality.isLoading} error={quality.error} />
        </Tabs.Content>
        <Tabs.Content value="financial" className="mt-4 outline-none">
          <KpiTable rows={financial.data} loading={financial.isLoading} error={financial.error} />
        </Tabs.Content>
        <Tabs.Content value="yield-and-scale" className="mt-4 outline-none">
          <KpiTable
            rows={yieldAndScale.data}
            loading={yieldAndScale.isLoading}
            error={yieldAndScale.error}
          />
        </Tabs.Content>
        <Tabs.Content value="technologies" className="mt-4 outline-none">
          <KpiTable
            rows={technologies.data}
            loading={technologies.isLoading}
            error={technologies.error}
          />
        </Tabs.Content>
        <Tabs.Content value="esg" className="mt-4 outline-none">
          <KpiTable rows={esg.data} loading={esg.isLoading} error={esg.error} />
        </Tabs.Content>
        <Tabs.Content value="esg-irregularities" className="mt-4 outline-none">
          <KpiTable
            rows={esgIrregularities.data}
            loading={esgIrregularities.isLoading}
            error={esgIrregularities.error}
          />
        </Tabs.Content>
      </Tabs.Root>
    </div>
  )
}

const KPI_TABLE_HIDDEN_COLUMNS = new Set(['cropSeasonId'])

function kpiTableColumnKeys(row: object): string[] {
  return Object.keys(row).filter((k) => !KPI_TABLE_HIDDEN_COLUMNS.has(k))
}

function kpiTableColumnLabel(key: string): string {
  if (key === 'cropSeasonCode') return 'Crop season'
  if (key === 'cultureTypeCode') return 'Culture type'
  if (key === 'technologyName') return 'Technology'
  if (key === 'irregularityTypeName') return 'Irregularity'
  if (key === 'deliveredAmountKg') return 'Delivered kg'
  if (key === 'contractedAmountKg') return 'Contracted kg'
  if (key === 'deliveredPercentage') return 'Delivered %'
  return key
}

function KpiTable({
  rows,
  loading,
  error,
}: {
  rows: unknown[] | undefined
  loading: boolean
  error: Error | null
}) {
  if (loading) return <p className="text-sm text-ink-muted">Loading…</p>
  if (error) return <p className="text-sm text-red-700">{(error as Error).message}</p>
  if (!rows?.length) return <p className="text-sm text-ink-muted">No rows for this season.</p>

  const keys = kpiTableColumnKeys(rows[0] as object)

  return (
    <div className="overflow-auto rounded-xl border border-black/5 bg-surface-card shadow-card">
      <table className="min-w-full text-left text-xs">
        <thead className="sticky top-0 border-b border-black/5 bg-surface-muted/90 backdrop-blur">
          <tr>
            {keys.map((k) => (
              <th key={k} className="whitespace-nowrap px-3 py-2 font-semibold text-ink-faint">
                {kpiTableColumnLabel(k)}
              </th>
            ))}
          </tr>
        </thead>
        <tbody className="divide-y divide-black/5">
          {rows.map((row, i) => (
            <tr key={i} className="hover:bg-surface-muted/30">
              {keys.map((k) => (
                <td key={k} className="whitespace-nowrap px-3 py-1.5 font-mono text-ink-muted">
                  {String((row as Record<string, unknown>)[k])}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}
