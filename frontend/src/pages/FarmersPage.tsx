import { useQuery } from '@tanstack/react-query'
import { Link } from 'react-router-dom'
import { apiGet } from '../api/client'
import type { FarmerListItemDto } from '../api/types'
import { useCropSeason } from '../context/CropSeasonContext'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function FarmersPage() {
  const { seasonId } = useCropSeason()

  const { data, isLoading, error } = useQuery({
    queryKey: ['farmers', seasonId],
    enabled: seasonId !== null,
    queryFn: () => apiGet<FarmerListItemDto[]>(`/api/Farmers?cropSeasonId=${seasonId}`),
  })

  if (seasonId === null) {
    return <p className="text-ink-muted">Loading seasons…</p>
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="font-display text-2xl font-bold text-ink">Farmers</h1>
        <Hint content={hints.farmersList} />
      </div>

      {isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
      {error && (
        <p className="text-sm text-red-700">
          {(error as Error).message}
        </p>
      )}

      {data && (
        <div className="overflow-hidden rounded-xl border border-black/5 bg-surface-card shadow-card">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-black/5 bg-surface-muted/80 text-xs font-semibold uppercase tracking-wide text-ink-faint">
              <tr>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Non-exclusive</th>
                <th className="px-4 py-3">Official score</th>
                <th className="px-4 py-3">Rank</th>
                <th className="px-4 py-3">Segment</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-black/5">
              {data.map((f) => (
                <tr key={f.farmerId} className="hover:bg-surface-muted/40">
                  <td className="px-4 py-2 font-mono text-xs">{f.farmerCode}</td>
                  <td className="px-4 py-2 font-medium text-ink">{f.farmerName}</td>
                  <td className="px-4 py-2 text-ink-muted">{f.nonExclusiveFarmer ? 'Yes' : 'No'}</td>
                  <td className="px-4 py-2 tabular-nums">
                    {f.totalScore ?? '—'}
                  </td>
                  <td className="px-4 py-2 tabular-nums">{f.rank ?? '—'}</td>
                  <td className="px-4 py-2 text-ink-muted">
                    {f.segmentName ?? '—'}
                  </td>
                  <td className="px-4 py-2 text-right">
                    <Link
                      to={`/farmers/${f.farmerId}?cropSeasonId=${seasonId}`}
                      className="text-leaf text-sm font-medium hover:underline"
                    >
                      Detail
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}
