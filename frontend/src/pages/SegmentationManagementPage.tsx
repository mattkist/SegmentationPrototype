import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useState } from 'react'
import { apiGet, apiPutEmpty, ApiRequestError } from '../api/client'
import type { SegmentationManagementRowDto } from '../api/types'
import { useCropSeason } from '../context/CropSeasonContext'
import { Hint } from '../components/Hint'
import { hints } from '../hints/en'

export function SegmentationManagementPage() {
  const { seasonId } = useCropSeason()
  const qc = useQueryClient()
  const [pendingSegmentByFarmer, setPendingSegmentByFarmer] = useState<
    Record<string, string | null>
  >({})
  const [actionMsg, setActionMsg] = useState<string | null>(null)

  const { data, isLoading, error } = useQuery({
    queryKey: ['segmentation-management', seasonId],
    enabled: seasonId !== null,
    queryFn: () =>
      apiGet<SegmentationManagementRowDto[]>(
        `/api/SegmentationManagement?cropSeasonId=${seasonId}`,
      ),
  })

  const submit = useMutation({
    mutationFn: async ({
      farmerId,
      segmentId,
    }: {
      farmerId: string
      segmentId: string | null
    }) => {
      await apiPutEmpty(
        `/api/SegmentationManagement/${farmerId}/crop-seasons/${seasonId}`,
        { segmentationConfigurationSegmentId: segmentId },
      )
    },
    onSuccess: (_, { farmerId }) => {
      setActionMsg('Segment submitted for approval.')
      setPendingSegmentByFarmer((prev) => {
        const next = { ...prev }
        delete next[farmerId]
        return next
      })
      void qc.invalidateQueries({ queryKey: ['segmentation-management', seasonId] })
      void qc.invalidateQueries({ queryKey: ['farmers', seasonId] })
    },
    onError: (e) => {
      setActionMsg(e instanceof ApiRequestError ? e.message : 'Submit failed.')
    },
  })

  if (seasonId === null) return <p className="text-ink-muted">Loading seasons…</p>

  const selectedSegment = (row: SegmentationManagementRowDto) => {
    if (row.farmerId in pendingSegmentByFarmer) {
      return pendingSegmentByFarmer[row.farmerId]
    }
    return row.segmentationConfigurationSegmentId
  }

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center gap-2">
        <h1 className="font-display text-2xl font-bold text-ink">Segmentation management</h1>
        <Hint content={hints.segmentationManagement} />
      </div>
      <p className="text-sm text-ink-muted">
        Review simulated scores for crop season {seasonId} and submit segment choices for approval.
      </p>

      {actionMsg && (
        <p className="text-sm text-ink-muted" role="status">
          {actionMsg}
        </p>
      )}

      {isLoading && <p className="text-sm text-ink-muted">Loading…</p>}
      {error && <p className="text-sm text-red-700">{(error as Error).message}</p>}

      {data && (
        <div className="overflow-auto rounded-xl border border-black/5 bg-surface-card shadow-card">
          <table className="min-w-full text-left text-sm">
            <thead className="border-b border-black/5 bg-surface-muted/80 text-xs font-semibold uppercase tracking-wide text-ink-faint">
              <tr>
                <th className="px-4 py-3">Code</th>
                <th className="px-4 py-3">Name</th>
                <th className="px-4 py-3">Culture</th>
                <th className="px-4 py-3">Score</th>
                <th className="px-4 py-3">Current segment</th>
                <th className="px-4 py-3">Segment</th>
                <th className="px-4 py-3" />
              </tr>
            </thead>
            <tbody className="divide-y divide-black/5">
              {data.map((row) => {
                const segmentId = selectedSegment(row)
                return (
                  <tr key={row.farmerId} className="hover:bg-surface-muted/40">
                    <td className="px-4 py-2 font-mono text-xs">{row.farmerCode}</td>
                    <td className="px-4 py-2 font-medium text-ink">{row.farmerName}</td>
                    <td className="px-4 py-2 font-mono text-xs text-ink-muted">
                      {row.cultureTypeCode}
                    </td>
                    <td className="px-4 py-2 tabular-nums">{row.totalScore}</td>
                    <td className="px-4 py-2 text-ink-muted">{row.segmentName ?? '—'}</td>
                    <td className="px-4 py-2">
                      <select
                        className="rounded-lg border border-black/10 px-2 py-1 text-sm"
                        value={segmentId ?? ''}
                        onChange={(e) => {
                          const v = e.target.value
                          setPendingSegmentByFarmer((prev) => ({
                            ...prev,
                            [row.farmerId]: v === '' ? null : v,
                          }))
                          setActionMsg(null)
                        }}
                      >
                        <option value="">No segment</option>
                        {row.availableSegments.map((s) => (
                          <option key={s.id ?? s.segmentName} value={s.id ?? ''}>
                            {s.segmentName}
                          </option>
                        ))}
                      </select>
                    </td>
                    <td className="px-4 py-2 text-right">
                      <button
                        type="button"
                        disabled={submit.isPending}
                        className="rounded-lg bg-leaf px-3 py-1.5 text-xs font-semibold text-white disabled:opacity-50"
                        onClick={() =>
                          submit.mutate({
                            farmerId: row.farmerId,
                            segmentId: selectedSegment(row),
                          })
                        }
                      >
                        Submit for approval
                      </button>
                    </td>
                  </tr>
                )
              })}
            </tbody>
          </table>
          {data.length === 0 && (
            <p className="p-6 text-sm text-ink-muted">
              No farmers with simulation results for this season.
            </p>
          )}
        </div>
      )}
    </div>
  )
}
