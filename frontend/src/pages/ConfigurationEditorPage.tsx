import { useCallback, useEffect, useRef, useState, type SetStateAction } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as Tabs from '@radix-ui/react-tabs'
import { apiGet, apiPatch, apiPost, apiPut, ApiRequestError } from '../api/client'
import type { SegmentationConfigurationDetailDto, SaveSegmentationConfigurationDto } from '../api/types'
import { createDefaultConfiguration } from '../domain/defaultConfiguration'
import { detailToSaveDto, bodyForSave, isOnlyConfigurationNameChange } from '../domain/configurationMapper'
import { deriveKpiMaxScores } from '../domain/deriveKpiMax'
import { syncRelevancesFromCaps } from '../domain/kpiRelevanceCaps'
import { KpiTotalsBanner } from '../features/configuration/KpiTotalsBanner'
import { SegmentsEditor } from '../features/configuration/SegmentsEditor'
import { LoyaltyEditor } from '../features/configuration/LoyaltyEditor'
import { QualityEditor } from '../features/configuration/QualityEditor'
import { FinancialEditor } from '../features/configuration/FinancialEditor'
import { EsgEditor, TechnologyEditor } from '../features/configuration/TechnologyEsgEditors'
import { ScaleEditor, YieldEditor } from '../features/configuration/YieldScaleEditors'
import { FieldLabel } from '../components/Hint'
import { cn } from '../lib/cn'

export function ConfigurationEditorPage() {
  const { id } = useParams<{ id: string }>()
  const isNew = id === 'new'
  const navigate = useNavigate()
  const qc = useQueryClient()
  const [draft, setDraft] = useState<SaveSegmentationConfigurationDto>(() =>
    createDefaultConfiguration(),
  )
  /** Edit mode: true only after server detail was applied to draft (avoids PUT with template segments missing ids). */
  const [draftHydratedFromServer, setDraftHydratedFromServer] = useState(isNew)
  const [saveError, setSaveError] = useState<string | null>(null)

  const setDraftNormalized = useCallback(
    (u: SetStateAction<SaveSegmentationConfigurationDto>) => {
      setDraft((prev) => {
        const next = typeof u === 'function' ? u(prev) : u
        return syncRelevancesFromCaps(next)
      })
    },
    [],
  )

  const detailQuery = useQuery({
    queryKey: ['config', id],
    enabled: !isNew && Boolean(id),
    queryFn: () => apiGet<SegmentationConfigurationDetailDto>(`/api/SegmentationConfigurations/${id!}`),
  })

  /** Snapshot of last server save payload (for rename-only PATCH). */
  const baselineSaveDto = useRef<SaveSegmentationConfigurationDto | null>(null)

  /** Avoid re-applying GET payload on every React Query refetch (would wipe unsaved edits). */
  const lastHydratedConfigId = useRef<string | null>(null)

  useEffect(() => {
    if (isNew) {
      setDraftNormalized(createDefaultConfiguration())
      setDraftHydratedFromServer(true)
      lastHydratedConfigId.current = null
      baselineSaveDto.current = null
      return
    }
    const d = detailQuery.data
    if (!d || String(d.id) !== String(id)) {
      setDraftHydratedFromServer(false)
      return
    }
    if (lastHydratedConfigId.current === String(id)) {
      setDraftHydratedFromServer(true)
      return
    }
    const next = detailToSaveDto(d)
    setDraftNormalized(next)
    baselineSaveDto.current = next
    setDraftHydratedFromServer(true)
    lastHydratedConfigId.current = String(id)
  }, [isNew, id, detailQuery.data])

  const saveMutation = useMutation({
    mutationFn: async () => {
      setSaveError(null)
      if (isNew) {
        return apiPost<SegmentationConfigurationDetailDto>(
          '/api/SegmentationConfigurations',
          bodyForSave(draft, 'create'),
        )
      }
      if (
        baselineSaveDto.current &&
        isOnlyConfigurationNameChange(draft, baselineSaveDto.current)
      ) {
        return apiPatch<SegmentationConfigurationDetailDto>(
          `/api/SegmentationConfigurations/${id}/name`,
          { name: draft.name },
        )
      }
      return apiPut<SegmentationConfigurationDetailDto>(
        `/api/SegmentationConfigurations/${id}`,
        bodyForSave(draft, 'update'),
      )
    },
    onSuccess: (data) => {
      void qc.invalidateQueries({ queryKey: ['configs'] })
      void qc.invalidateQueries({ queryKey: ['config', data.id] })
      if (isNew) navigate(`/configurations/${data.id}`, { replace: true })
      else {
        const next = detailToSaveDto(data)
        setDraftNormalized(next)
        baselineSaveDto.current = next
        lastHydratedConfigId.current = String(data.id)
      }
    },
    onError: (e) => {
      if (e instanceof ApiRequestError) {
        const b = e.body as { message?: string; sumOfKpiMaxScores?: number; maximumScore?: number } | null
        if (b && typeof b === 'object' && 'sumOfKpiMaxScores' in b) {
          setSaveError(
            `${b.message ?? 'Validation failed'} (sum ${b.sumOfKpiMaxScores}, target ${b.maximumScore})`,
          )
        } else {
          setSaveError(e.message)
        }
      } else setSaveError((e as Error).message)
    },
  })

  if (!isNew && detailQuery.isLoading) {
    return <p className="text-sm text-ink-muted">Loading configuration…</p>
  }
  if (!isNew && detailQuery.error) {
    return <p className="text-sm text-red-700">{(detailQuery.error as Error).message}</p>
  }

  const canSave =
    draftHydratedFromServer &&
    deriveKpiMaxScores(draft).matchesMaximum &&
    draft.segments.length > 0

  return (
    <div className="mx-auto max-w-5xl space-y-6">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <Link to="/configurations" className="text-sm font-medium text-leaf hover:underline">
          ← Configurations
        </Link>
        <button
          type="button"
          disabled={!canSave || saveMutation.isPending}
          onClick={() => saveMutation.mutate()}
          className={cn(
            'rounded-xl px-5 py-2 text-sm font-semibold text-white shadow-sm transition',
            canSave ? 'bg-leaf hover:bg-leaf-hover' : 'cursor-not-allowed bg-ink-faint',
          )}
        >
          {saveMutation.isPending ? 'Saving…' : isNew ? 'Create' : 'Save changes'}
        </button>
      </div>

      <label className="block max-w-xl text-sm">
        <FieldLabel label="Configuration name" hint="Shown in lists and simulation picker." />
        <input
          className="mt-1 w-full rounded-lg border border-black/10 px-3 py-2 text-sm"
          value={draft.name}
          onChange={(e) =>
            setDraftNormalized((d) => (d ? { ...d, name: e.target.value } : d))
          }
        />
      </label>

        <KpiTotalsBanner draft={draft} setDraft={setDraftNormalized} />

      {saveError && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-900">
          {saveError}
        </div>
      )}

      <Tabs.Root defaultValue="overview" className="space-y-4">
        <Tabs.List className="flex flex-wrap gap-1 rounded-xl border border-black/5 bg-surface-card p-1">
          {[
            ['overview', 'Overview'],
            ['loyalty', 'Loyalty'],
            ['quality', 'Quality'],
            ['financial', 'Financial'],
            ['technology', 'Technology'],
            ['esg', 'ESG'],
            ['yield', 'Yield'],
            ['scale', 'Scale'],
          ].map(([value, label]) => (
            <Tabs.Trigger
              key={value}
              value={value}
              className="rounded-lg px-3 py-2 text-sm font-medium data-[state=active]:bg-leaf data-[state=active]:text-white data-[state=inactive]:text-ink-muted data-[state=inactive]:hover:bg-surface-muted"
            >
              {label}
            </Tabs.Trigger>
          ))}
        </Tabs.List>
        <Tabs.Content value="overview" className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card outline-none">
          <SegmentsEditor draft={draft} setDraft={setDraftNormalized} />
        </Tabs.Content>
        <Tabs.Content value="loyalty" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <LoyaltyEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="quality" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <QualityEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="financial" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <FinancialEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="technology" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <TechnologyEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="esg" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <EsgEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="yield" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <YieldEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
        <Tabs.Content value="scale" className="outline-none">
          <div className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card">
            <ScaleEditor draft={draft} setDraft={setDraftNormalized} />
          </div>
        </Tabs.Content>
      </Tabs.Root>
    </div>
  )
}
