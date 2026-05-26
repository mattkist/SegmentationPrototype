import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { apiGet, apiPost, ApiRequestError } from '../api/client'
import type {
  CreateSegmentationSimulationDto,
  CropSeasonDto,
  SegmentationConfigurationSummaryDto,
  SegmentationSimulationDetailDto,
  SegmentationSimulationSummaryDto,
} from '../api/types'
import { useCropSeason } from '../context/CropSeasonContext'
import { Hint } from '../components/Hint'
import { KpiScopeFormSection } from '../features/simulation/KpiScopeFormSection'
import {
  buildKpiScopesFromForm,
  createDefaultKpiScopeForm,
  formatKpiScopesSummary,
  isKpiScopeFormComplete,
} from '../features/simulation/kpiScopeUtils'
import { hints } from '../hints/en'

export function SimulationsPage() {
  const { seasonId } = useCropSeason()
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [configId, setConfigId] = useState('')
  const [kpiScopeForm, setKpiScopeForm] = useState(createDefaultKpiScopeForm)

  const list = useQuery({
    queryKey: ['sims', seasonId],
    enabled: seasonId !== null,
    queryFn: () =>
      apiGet<SegmentationSimulationSummaryDto[]>(
        `/api/SegmentationSimulations?cropSeasonId=${seasonId}`,
      ),
  })

  const configs = useQuery({
    queryKey: ['configs'],
    queryFn: () => apiGet<SegmentationConfigurationSummaryDto[]>('/api/SegmentationConfigurations'),
  })

  const seasons = useQuery({
    queryKey: ['cropSeasons'],
    queryFn: () => apiGet<CropSeasonDto[]>('/api/CropSeasons'),
  })

  const create = useMutation({
    mutationFn: (body: CreateSegmentationSimulationDto) =>
      apiPost<SegmentationSimulationDetailDto>('/api/SegmentationSimulations', body),
    onSuccess: (row) => {
      void qc.invalidateQueries({ queryKey: ['sims', seasonId] })
      navigate(`/simulations/${row.id}`)
    },
  })

  if (seasonId === null) return <p className="text-ink-muted">Loading seasons…</p>

  const seasonList = seasons.data ?? []
  const scopeComplete = isKpiScopeFormComplete(kpiScopeForm)

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-2">
        <h1 className="font-display text-2xl font-bold text-ink">Simulations</h1>
        <Hint content={hints.homeSimulations} />
      </div>

      <section className="rounded-xl border border-black/5 bg-surface-card p-5 shadow-card">
        <h2 className="flex items-center gap-2 text-sm font-semibold text-ink">
          New simulation
          <Hint content={hints.simulationCreate} />
        </h2>
        <p className="mt-1 text-xs text-ink-muted">
          Target season (header): {seasonId}. Configure crop seasons and aggregation per KPI.
        </p>
        <p className="mt-2 flex items-start gap-2 rounded-lg border border-sky-200/60 bg-sky-50/60 px-3 py-2 text-xs text-ink-muted">
          <Hint content={hints.simulationNewFarmer} />
          <span>{hints.simulationNewFarmerShort}</span>
        </p>
        <div className="mt-4 flex flex-wrap items-end gap-3">
          <label className="block min-w-[220px] text-sm">
            <span className="text-ink-muted">Configuration</span>
            <select
              className="mt-1 w-full rounded-lg border border-black/10 px-3 py-2 text-sm"
              value={configId}
              onChange={(e) => setConfigId(e.target.value)}
            >
              <option value="">Select…</option>
              {(configs.data ?? []).map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name} ({c.cultureTypeCodes.join(', ')})
                </option>
              ))}
            </select>
          </label>
        </div>
        <div className="mt-4">
          {seasonList.length > 0 ? (
            <KpiScopeFormSection
              seasons={seasonList}
              form={kpiScopeForm}
              onChange={setKpiScopeForm}
            />
          ) : (
            <p className="mt-2 text-sm text-ink-muted">Loading crop seasons…</p>
          )}
        </div>
        <button
          type="button"
          disabled={!configId || !scopeComplete || create.isPending}
          className="mt-4 rounded-xl bg-leaf px-4 py-2 text-sm font-semibold text-white disabled:opacity-50"
          onClick={() =>
            create.mutate({
              segmentationConfigurationId: configId,
              cropSeasonId: seasonId,
              kpiScopes: buildKpiScopesFromForm(kpiScopeForm),
            })
          }
        >
          {create.isPending ? 'Running…' : 'Run simulation'}
        </button>
        {create.error && (
          <p className="mt-2 text-sm text-red-700">
            {create.error instanceof ApiRequestError ? create.error.message : 'Failed'}
          </p>
        )}
      </section>

      <section>
        <h2 className="mb-2 text-sm font-semibold text-ink-muted">History for this season</h2>
        {list.isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
        {list.error && (
          <p className="text-sm text-red-700">{(list.error as Error).message}</p>
        )}
        {list.data && (
          <div className="overflow-hidden rounded-xl border border-black/5 bg-surface-card shadow-card">
            <table className="w-full text-left text-sm">
              <thead className="border-b border-black/5 bg-surface-muted/80 text-xs font-semibold uppercase text-ink-faint">
                <tr>
                  <th className="px-4 py-3">Date</th>
                  <th className="px-4 py-3">Configuration</th>
                  <th className="px-4 py-3">KPI scopes</th>
                  <th className="px-4 py-3">Status</th>
                  <th className="px-4 py-3">Farmers</th>
                  <th className="px-4 py-3" />
                </tr>
              </thead>
              <tbody className="divide-y divide-black/5">
                {list.data.map((s) => (
                  <tr key={s.id} className="hover:bg-surface-muted/40">
                    <td className="px-4 py-2 text-xs text-ink-muted">
                      {new Date(s.simulationDate).toLocaleString()}
                    </td>
                    <td className="px-4 py-2">{s.configurationName}</td>
                    <td className="max-w-md px-4 py-2 text-xs text-ink-muted">
                      {formatKpiScopesSummary(s.kpiScopes, seasonList)}
                    </td>
                    <td className="px-4 py-2 font-mono">{s.status}</td>
                    <td className="px-4 py-2 tabular-nums">{s.farmerCount}</td>
                    <td className="px-4 py-2 text-right">
                      <Link className="text-leaf text-sm font-medium hover:underline" to={`/simulations/${s.id}`}>
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            {list.data.length === 0 && (
              <p className="p-6 text-sm text-ink-muted">No simulations for this season yet.</p>
            )}
          </div>
        )}
      </section>
    </div>
  )
}
