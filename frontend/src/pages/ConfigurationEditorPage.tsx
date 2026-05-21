import { useCallback, useEffect, useMemo, useRef, useState, type SetStateAction } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Link, useNavigate, useParams } from 'react-router-dom'
import * as Tabs from '@radix-ui/react-tabs'
import { apiGet, apiPatch, apiPost, apiPut, ApiRequestError } from '../api/client'
import type { SegmentationConfigurationDetailDto, SaveSegmentationConfigurationDto } from '../api/types'
import { createDefaultConfiguration } from '../domain/defaultConfiguration'
import { detailToSaveDto, bodyForSave, isOnlyConfigurationNameChange } from '../domain/configurationMapper'
import { deriveKpiMaxScores } from '../domain/deriveKpiMax'
import {
  draftHasAllCatalogCultureTypes,
  mergeCultureTypesFromCatalog,
  normalizeCultureTypesCatalog,
  type CultureTypeCatalogItem,
} from '../domain/cultureTypeDraft'
import { CultureTypePanel } from '../features/configuration/CultureTypePanel'
import { SegmentsEditor } from '../features/configuration/SegmentsEditor'
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
  const [draftHydratedFromServer, setDraftHydratedFromServer] = useState(isNew)
  const [saveError, setSaveError] = useState<string | null>(null)
  const [activeCultureCode, setActiveCultureCode] = useState('FCV')

  const cultureTypesQuery = useQuery({
    queryKey: ['cultureTypes'],
    queryFn: async () =>
      normalizeCultureTypesCatalog(await apiGet<unknown>('/api/CultureTypes')),
    staleTime: 60_000,
  })

  const catalogRef = useRef<CultureTypeCatalogItem[]>([])
  catalogRef.current = cultureTypesQuery.data ?? []

  const setDraftNormalized = useCallback((u: SetStateAction<SaveSegmentationConfigurationDto>) => {
    setDraft((prev) => {
      const next = typeof u === 'function' ? u(prev) : u
      const catalog = catalogRef.current
      return catalog.length > 0 ? mergeCultureTypesFromCatalog(next, catalog) : next
    })
  }, [])

  const detailQuery = useQuery({
    queryKey: ['config', id],
    enabled: !isNew && Boolean(id),
    queryFn: () => apiGet<SegmentationConfigurationDetailDto>(`/api/SegmentationConfigurations/${id!}`),
  })

  const baselineSaveDto = useRef<SaveSegmentationConfigurationDto | null>(null)
  const lastHydratedConfigId = useRef<string | null>(null)
  const newConfigInitialized = useRef(false)

  useEffect(() => {
    const catalog = cultureTypesQuery.data
    if (!catalog?.length) return
    if (draftHasAllCatalogCultureTypes(draft, catalog)) return
    setDraftNormalized((d) => d)
  }, [cultureTypesQuery.data, draft.cultureTypes, setDraftNormalized])

  useEffect(() => {
    const codes = cultureTypesQuery.data?.map((c) => c.code) ?? []
    if (codes.length > 0 && !codes.includes(activeCultureCode)) {
      setActiveCultureCode(codes[0])
    }
  }, [cultureTypesQuery.data, activeCultureCode])

  useEffect(() => {
    if (!isNew) {
      newConfigInitialized.current = false
      return
    }
    if (newConfigInitialized.current) return
    newConfigInitialized.current = true
    setDraftNormalized(createDefaultConfiguration())
    setDraftHydratedFromServer(true)
    lastHydratedConfigId.current = null
    baselineSaveDto.current = null
  }, [isNew, setDraftNormalized])

  useEffect(() => {
    if (isNew) return
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
    const tabCodes = cultureTypesQuery.data?.map((c) => c.code) ?? d.cultureTypes.map((c) => c.cultureTypeCode)
    if (tabCodes.length > 0) setActiveCultureCode(tabCodes[0])
  }, [isNew, id, detailQuery.data, setDraftNormalized, cultureTypesQuery.data])

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

  const cultureTabCodes = useMemo(() => {
    const fromCatalog = cultureTypesQuery.data?.map((c) => c.code) ?? []
    if (fromCatalog.length > 0) return fromCatalog
    return draft.cultureTypes.map((c) => c.cultureTypeCode)
  }, [cultureTypesQuery.data, draft.cultureTypes])

  if (!isNew && detailQuery.isLoading) {
    return <p className="text-sm text-ink-muted">Loading configuration…</p>
  }
  if (!isNew && detailQuery.error) {
    return <p className="text-sm text-red-700">{(detailQuery.error as Error).message}</p>
  }

  const canSave =
    draftHydratedFromServer &&
    draft.segments.length > 0 &&
    draft.cultureTypes.length > 0 &&
    draft.cultureTypes.every((ct) => deriveKpiMaxScores(ct).matchesMaximum)

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

      <section className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card space-y-6">
        <label className="block max-w-xl text-sm">
          <FieldLabel label="Configuration name" hint="Shown in lists and simulation picker." />
          <input
            className="mt-1 w-full rounded-lg border border-black/10 px-3 py-2 text-sm"
            value={draft.name}
            onChange={(e) => setDraftNormalized((d) => ({ ...d, name: e.target.value }))}
          />
        </label>
        <SegmentsEditor draft={draft} setDraft={setDraftNormalized} />
      </section>

      {saveError && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-900">
          {saveError}
        </div>
      )}

      {cultureTypesQuery.isError && (
        <p className="text-sm text-amber-800">
          Could not load culture types from the API. Only culture types already in this configuration are shown.
        </p>
      )}

      {cultureTabCodes.length > 0 && (
        <Tabs.Root value={activeCultureCode} onValueChange={setActiveCultureCode} className="space-y-4">
          <Tabs.List className="flex flex-wrap gap-1 rounded-xl border border-black/5 bg-surface-card p-1">
            {cultureTabCodes.map((code) => (
              <Tabs.Trigger
                key={code}
                value={code}
                className={cn(
                  'rounded-lg px-4 py-2 text-sm font-semibold',
                  'data-[state=active]:bg-leaf data-[state=active]:text-white',
                  'data-[state=inactive]:text-ink-muted data-[state=inactive]:hover:bg-surface-muted',
                )}
              >
                {code}
              </Tabs.Trigger>
            ))}
          </Tabs.List>
          {cultureTabCodes.map((code) => (
            <Tabs.Content key={code} value={code} className="outline-none">
              {draft.cultureTypes.some((c) => c.cultureTypeCode === code) ? (
                <CultureTypePanel
                  draft={draft}
                  setDraft={setDraftNormalized}
                  cultureTypeCode={code}
                />
              ) : (
                <p className="text-sm text-ink-muted">Preparing {code} settings…</p>
              )}
            </Tabs.Content>
          ))}
        </Tabs.Root>
      )}
    </div>
  )
}
