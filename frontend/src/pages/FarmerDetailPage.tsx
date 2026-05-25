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
      <Link to={`/farmers`} className="text-sm font-medium text-leaf hover:underline">
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
              <KpiCard title="Yield & Scale" row={data.kpis.yieldAndScale} />
              <KpiCard title="ESG" row={data.kpis.esg} />
            </div>
            {data.kpis.technologies.length > 0 && (
              <div className="mt-4">
                <h3 className="text-xs font-semibold uppercase text-ink-faint">Technologies</h3>
                <ul className="mt-2 space-y-1 text-sm text-ink-muted">
                  {data.kpis.technologies.map((t, i) => (
                    <li key={i}>
                      {t.technologyName} ({t.cultureTypeCode})
                    </li>
                  ))}
                </ul>
              </div>
            )}
            {data.kpis.esgIrregularities.length > 0 && (
              <div className="mt-4">
                <h3 className="text-xs font-semibold uppercase text-ink-faint">
                  ESG irregularities
                </h3>
                <ul className="mt-2 space-y-1 text-sm text-ink-muted">
                  {data.kpis.esgIrregularities.map((t, i) => (
                    <li key={i}>
                      {t.irregularityTypeName} ({t.cultureTypeCode})
                    </li>
                  ))}
                </ul>
              </div>
            )}
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
  if (!row) {
    return (
      <div className="rounded-lg border border-dashed border-black/10 p-3">
        <h3 className="text-xs font-semibold text-ink-faint">{title}</h3>
        <p className="mt-1 text-sm text-ink-muted">No data</p>
      </div>
    )
  }
  const entries = Object.entries(row as Record<string, unknown>).filter(
    ([k]) => !['farmerCode', 'cropSeasonId', 'cropSeasonCode'].includes(k),
  )
  return (
    <div className="rounded-lg border border-black/5 bg-surface-muted/30 p-3">
      <h3 className="text-xs font-semibold text-ink-faint">{title}</h3>
      <dl className="mt-2 space-y-1 text-xs">
        {entries.map(([k, v]) => (
          <div key={k} className="flex justify-between gap-2">
            <dt className="text-ink-faint">{k}</dt>
            <dd className="font-mono text-ink-muted">{String(v)}</dd>
          </div>
        ))}
      </dl>
    </div>
  )
}
