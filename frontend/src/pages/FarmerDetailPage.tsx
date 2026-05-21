import { useQuery } from '@tanstack/react-query'
import { Link, useParams, useSearchParams } from 'react-router-dom'
import { apiGet } from '../api/client'
import type { FarmerDetailDto } from '../api/types'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function FarmerDetailPage() {
  const { farmerId } = useParams<{ farmerId: string }>()
  const [sp] = useSearchParams()
  const cropSeasonId = Number(sp.get('cropSeasonId') || '')

  const { data, isLoading, error } = useQuery({
    queryKey: ['farmer', farmerId, cropSeasonId],
    enabled: Boolean(farmerId) && Number.isFinite(cropSeasonId),
    queryFn: () =>
      apiGet<FarmerDetailDto>(`/api/Farmers/${farmerId}?cropSeasonId=${cropSeasonId}`),
  })

  if (!farmerId || !Number.isFinite(cropSeasonId)) {
    return (
      <p className="text-ink-muted">
        Missing farmer or crop season. Open from the farmers list.
      </p>
    )
  }

  return (
    <div className="mx-auto max-w-4xl space-y-6">
      <Link
        to={`/farmers`}
        className="text-sm font-medium text-leaf hover:underline"
      >
        ← Farmers
      </Link>
      <div className="flex items-center gap-2">
        <h1 className="font-display text-2xl font-bold text-ink">
          {data ? `${data.farmerName} (${data.farmerCode})` : 'Farmer'}
        </h1>
        <Hint content={hints.farmerDetail} />
      </div>

      {isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
      {error && <p className="text-sm text-red-700">{(error as Error).message}</p>}

      {data && (
        <>
          <section className="rounded-xl border border-black/5 bg-surface-card p-5 shadow-card">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-ink-faint">
              Official segmentation
            </h2>
            {data.officialSegmentation ? (
              <dl className="mt-3 grid gap-2 text-sm sm:grid-cols-2">
                <div>
                  <dt className="text-ink-faint">Total</dt>
                  <dd className="font-semibold tabular-nums text-ink">
                    {data.officialSegmentation.totalScore}
                  </dd>
                </div>
                <div>
                  <dt className="text-ink-faint">Rank</dt>
                  <dd className="font-semibold tabular-nums text-ink">
                    {data.officialSegmentation.rank}
                  </dd>
                </div>
                <div className="sm:col-span-2">
                  <dt className="text-ink-faint">Segment</dt>
                  <dd className="font-medium text-ink">
                    {data.officialSegmentation.segmentName ?? '—'}
                  </dd>
                </div>
                <div className="sm:col-span-2 border-t border-black/5 pt-3">
                  <dt className="mb-1 text-ink-faint">Component scores</dt>
                  <dd className="grid grid-cols-2 gap-x-4 gap-y-1 font-mono text-xs text-ink-muted sm:grid-cols-4">
                    <span>Loyalty {data.officialSegmentation.loyaltyScore}</span>
                    <span>Quality {data.officialSegmentation.qualityScore}</span>
                    <span>Financial {data.officialSegmentation.financialScore}</span>
                    <span>Tech {data.officialSegmentation.technologiesScore}</span>
                    <span>ESG {data.officialSegmentation.esgScore}</span>
                    <span>Yield {data.officialSegmentation.yieldScore}</span>
                    <span>Scale {data.officialSegmentation.scaleScore}</span>
                    <span>Y&amp;S {data.officialSegmentation.yieldAndScaleScore}</span>
                  </dd>
                </div>
              </dl>
            ) : (
              <p className="mt-2 text-sm text-ink-muted">
                No official segmentation for {data.cropSeasonCode}. Run a simulation and accept
                it, or pick another season.
              </p>
            )}
          </section>

          <section className="rounded-xl border border-black/5 bg-surface-card p-5 shadow-card">
            <h2 className="text-sm font-semibold uppercase tracking-wide text-ink-faint">
              KPIs — {data.cropSeasonCode}
            </h2>
            <div className="mt-4 grid gap-4 sm:grid-cols-2">
              <KpiCard title="Loyalty" row={data.kpis.loyalty} />
              <KpiCard title="Quality" row={data.kpis.quality} />
              <KpiCard title="Financial" row={data.kpis.financial} />
              <KpiCard title="Technologies" row={data.kpis.technologies} />
              <KpiCard title="ESG" row={data.kpis.esg} />
              <KpiCard title="Yield" row={data.kpis.yield} />
              <KpiCard title="Scale" row={data.kpis.scale} />
            </div>
          </section>
        </>
      )}
    </div>
  )
}

function KpiCard({
  title,
  row,
}: {
  title: string
  row: unknown
}) {
  return (
    <div className="rounded-lg border border-black/5 bg-surface p-3">
      <div className="text-xs font-semibold uppercase tracking-wide text-ink-faint">{title}</div>
      {row && typeof row === 'object' ? (
        <pre className="mt-2 max-h-40 overflow-auto text-[11px] leading-relaxed text-ink-muted">
          {JSON.stringify(row, null, 2)}
        </pre>
      ) : (
        <p className="mt-2 text-sm text-ink-muted">No row for this season.</p>
      )}
    </div>
  )
}
