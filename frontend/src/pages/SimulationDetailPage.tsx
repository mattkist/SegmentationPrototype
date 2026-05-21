import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useParams } from 'react-router-dom'
import { apiGet, apiPostEmpty, ApiRequestError } from '../api/client'
import type { SegmentationSimulationDetailDto } from '../api/types'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function SimulationDetailPage() {
  const { id } = useParams<{ id: string }>()
  const qc = useQueryClient()

  const { data, isLoading, error } = useQuery({
    queryKey: ['sim', id],
    enabled: Boolean(id),
    queryFn: () => apiGet<SegmentationSimulationDetailDto>(`/api/SegmentationSimulations/${id}`),
  })

  const accept = useMutation({
    mutationFn: () => apiPostEmpty(`/api/SegmentationSimulations/${id}/accept-official`),
    onSuccess: () => {
      void qc.invalidateQueries({ queryKey: ['sim', id] })
      void qc.invalidateQueries({ queryKey: ['sims'] })
      void qc.invalidateQueries({ queryKey: ['farmers'] })
    },
  })

  if (!id) return <p className="text-ink-muted">Missing id.</p>

  return (
    <div className="space-y-6">
      <Link to="/simulations" className="text-sm font-medium text-leaf hover:underline">
        ← Simulations
      </Link>
      {isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
      {error && <p className="text-sm text-red-700">{(error as Error).message}</p>}

      {data && (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <h1 className="font-display text-2xl font-bold text-ink">
              {data.configurationName}
            </h1>
            <span className="rounded-full bg-surface-muted px-3 py-1 font-mono text-xs">
              {data.cropSeasonCode} · {data.status === 'O' ? 'Official' : 'Simulation'}
            </span>
          </div>
          <p className="text-sm text-ink-muted">
            {new Date(data.simulationDate).toLocaleString()}
          </p>

          <div className="flex flex-wrap items-center gap-3">
            {data.status === 'S' && (
              <button
                type="button"
                disabled={accept.isPending}
                className="rounded-xl bg-accent px-4 py-2 text-sm font-semibold text-white shadow-sm hover:opacity-95 disabled:opacity-50"
                onClick={() => {
                  if (
                    window.confirm(
                      'Accept this simulation as the official segmentation for this crop season? Other official runs for the same season will be demoted.',
                    )
                  ) {
                    accept.mutate()
                  }
                }}
              >
                {accept.isPending ? 'Accepting…' : 'Accept as official'}
              </button>
            )}
            <div className="flex items-center gap-2 text-sm text-ink-muted">
              <Hint content={hints.simulationAccept} />
            </div>
          </div>
          {accept.error && (
            <p className="text-sm text-red-700">
              {accept.error instanceof ApiRequestError ? accept.error.message : 'Accept failed'}
            </p>
          )}

          <div className="overflow-auto rounded-xl border border-black/5 bg-surface-card shadow-card">
            <table className="min-w-full text-left text-xs">
              <thead className="sticky top-0 border-b border-black/5 bg-surface-muted/95 text-[10px] font-semibold uppercase tracking-wide text-ink-faint backdrop-blur">
                <tr>
                  <th className="px-3 py-2">Culture</th>
                  <th className="px-3 py-2">Farmer</th>
                  <th className="px-3 py-2">Total</th>
                  <th className="px-3 py-2">Loy</th>
                  <th className="px-3 py-2">Qual</th>
                  <th className="px-3 py-2">Fin</th>
                  <th className="px-3 py-2">Tech</th>
                  <th className="px-3 py-2">ESG</th>
                  <th className="px-3 py-2">Yld</th>
                  <th className="px-3 py-2">Scl</th>
                  <th className="px-3 py-2">Y&amp;S</th>
                  <th className="px-3 py-2">
                    <span className="inline-flex items-center gap-1">
                      Segment
                      <Hint content={hints.simulationSegment} />
                    </span>
                  </th>
                  <th className="px-3 py-2">
                    <span className="inline-flex items-center gap-1">
                      NE
                      <Hint content="Non-exclusive farmer flag at scoring time." />
                    </span>
                  </th>
                </tr>
              </thead>
              <tbody className="divide-y divide-black/5">
                {data.farmers.map((f) => (
                  <tr key={f.farmerId} className="hover:bg-surface-muted/30">
                    <td className="px-3 py-1.5 font-mono text-ink-muted">{f.cultureTypeCode}</td>
                    <td className="px-3 py-1.5">
                      <div className="font-medium text-ink">{f.farmerName}</div>
                      <div className="font-mono text-[10px] text-ink-faint">{f.farmerCode}</div>
                    </td>
                    <td className="px-3 py-1.5 font-mono tabular-nums">{f.totalScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.loyaltyScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.qualityScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.financialScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.technologiesScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.esgScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.yieldScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.scaleScore}</td>
                    <td className="px-3 py-1.5 font-mono tabular-nums text-ink-muted">{f.yieldAndScaleScore}</td>
                    <td className="px-3 py-1.5 text-ink-muted">{f.segmentName ?? '—'}</td>
                    <td className="px-3 py-1.5">{f.nonExclusiveFarmer ? 'Y' : 'N'}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  )
}
