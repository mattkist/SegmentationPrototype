import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate } from 'react-router-dom'
import { apiGet, apiPost, ApiRequestError } from '../api/client'
import type { SegmentationConfigurationDetailDto, SegmentationConfigurationSummaryDto } from '../api/types'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function ConfigurationsPage() {
  const navigate = useNavigate()
  const qc = useQueryClient()

  const { data, isLoading, error } = useQuery({
    queryKey: ['configs'],
    queryFn: () => apiGet<SegmentationConfigurationSummaryDto[]>('/api/SegmentationConfigurations'),
  })

  const duplicate = useMutation({
    mutationFn: (configId: string) =>
      apiPost<SegmentationConfigurationDetailDto>(
        `/api/SegmentationConfigurations/${configId}/duplicate`,
        {},
      ),
    onSuccess: (created) => {
      void qc.invalidateQueries({ queryKey: ['configs'] })
      navigate(`/configurations/${created.id}`)
    },
  })

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-2">
          <h1 className="font-display text-2xl font-bold text-ink">Configurations</h1>
          <Hint content={hints.homeConfigs} />
        </div>
        <Link
          to="/configurations/new"
          className="rounded-xl bg-leaf px-4 py-2 text-sm font-semibold text-white shadow-sm hover:bg-leaf-hover"
        >
          New configuration
        </Link>
      </div>

      {isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
      {error && <p className="text-sm text-red-700">{(error as Error).message}</p>}
      {duplicate.error && (
        <p className="text-sm text-red-700">
          {duplicate.error instanceof ApiRequestError
            ? duplicate.error.message
            : 'Duplicate failed.'}
        </p>
      )}

      {data && (
        <div className="overflow-hidden rounded-xl border border-black/5 bg-surface-card shadow-card">
          <table className="w-full text-left text-sm">
            <thead className="border-b border-black/5 bg-surface-muted/80 text-xs font-semibold uppercase tracking-wide text-ink-faint">
              <tr>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Culture type</th>
                <th className="px-4 py-3">Maximum score</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-black/5">
              {data.map((c) => (
                <tr key={c.id} className="hover:bg-surface-muted/40">
                  <td className="px-4 py-2 font-medium text-ink">{c.name}</td>
                  <td className="px-4 py-2 text-ink-muted">{c.cultureTypeCode}</td>
                  <td className="px-4 py-2 font-mono tabular-nums text-ink-muted">{c.maximumScore}</td>
                  <td className="px-4 py-2 text-right">
                    <Link
                      className="mr-3 text-leaf text-sm font-medium hover:underline"
                      to={`/configurations/${c.id}`}
                    >
                      Edit
                    </Link>
                    <button
                      type="button"
                      disabled={duplicate.isPending}
                      className="text-sm font-medium text-ink-muted hover:text-accent"
                      onClick={() => duplicate.mutate(c.id)}
                    >
                      Duplicate
                    </button>
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
