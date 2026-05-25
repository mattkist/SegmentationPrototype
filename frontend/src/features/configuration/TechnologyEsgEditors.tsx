import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../../api/client'
import type {
  IrregularityTypeDto,
  SaveSegmentationConfigurationDto,
  TechnologyDto,
} from '../../api/types'
import { InlineDecimal, InlineInt, PointsLabel, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

export function TechnologyEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const catalog = useQuery({
    queryKey: ['technologies'],
    queryFn: () => apiGet<TechnologyDto[]>('/api/ReferenceData/technologies'),
    staleTime: 60_000,
  })

  const scores = block.technology.technologyScores

  const upsertScore = (technologyId: number, score: number) => {
    patchBlock((b) => {
      const list = [...b.technology.technologyScores]
      const idx = list.findIndex((x) => x.technologyId === technologyId)
      if (idx >= 0) list[idx] = { technologyId, score }
      else list.push({ technologyId, score })
      return { ...b, technology: { ...b.technology, technologyScores: list } }
    })
  }

  const addTechnology = (technologyId: number) => {
    if (scores.some((s) => s.technologyId === technologyId)) return
    patchBlock((b) => ({
      ...b,
      technology: {
        ...b.technology,
        technologyScores: [...b.technology.technologyScores, { technologyId, score: 0 }],
      },
    }))
  }

  const removeTechnology = (technologyId: number) => {
    patchBlock((b) => ({
      ...b,
      technology: {
        ...b.technology,
        technologyScores: b.technology.technologyScores.filter(
          (s) => s.technologyId !== technologyId,
        ),
      },
    }))
  }

  const configuredIds = new Set(scores.map((s) => s.technologyId))
  const available = (catalog.data ?? []).filter((t) => !configuredIds.has(t.id))

  return (
    <section className="space-y-4">
      <SectionTitle
        title="Technology"
        hint={`${hints.technology}\n\nSimulation awards points for each technology present in the latest scope season's Technologies KPI rows.`}
      />
      {catalog.isLoading && <p className="text-sm text-ink-muted">Loading technology catalog…</p>}
      {scores.length === 0 && (
        <p className="text-sm text-ink-muted">Add a technology and set its score.</p>
      )}
      {scores.map((row) => {
        const name =
          catalog.data?.find((t) => t.id === row.technologyId)?.name ??
          `Technology #${row.technologyId}`
        return (
          <div key={row.technologyId} className="flex flex-wrap items-center gap-2">
            <RuleSentence>
              If has {name} =
              <InlineInt value={row.score} onChange={(v) => upsertScore(row.technologyId, v)} />
              <PointsLabel />
            </RuleSentence>
            <button
              type="button"
              className="text-xs text-amber-700 hover:underline"
              onClick={() => removeTechnology(row.technologyId)}
            >
              Remove
            </button>
          </div>
        )
      })}
      {available.length > 0 && (
        <label className="flex flex-wrap items-center gap-2 text-sm">
          <span className="text-ink-muted">Add technology</span>
          <select
            className="rounded-lg border border-black/10 px-2 py-1"
            defaultValue=""
            onChange={(e) => {
              const id = Number(e.target.value)
              e.target.value = ''
              if (Number.isFinite(id)) addTechnology(id)
            }}
          >
            <option value="">Select…</option>
            {available.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
        </label>
      )}
    </section>
  )
}

export function EsgEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const irregularityCatalog = useQuery({
    queryKey: ['irregularity-types'],
    queryFn: () => apiGet<IrregularityTypeDto[]>('/api/ReferenceData/irregularity-types'),
    staleTime: 60_000,
  })

  const e = block.esg
  const irrScores = e.irregularityScores

  const upsertIrregularity = (irregularityTypeId: number, score: number) => {
    patchBlock((b) => {
      const list = [...b.esg.irregularityScores]
      const idx = list.findIndex((x) => x.irregularityTypeId === irregularityTypeId)
      if (idx >= 0) list[idx] = { irregularityTypeId, score }
      else list.push({ irregularityTypeId, score })
      return { ...b, esg: { ...b.esg, irregularityScores: list } }
    })
  }

  const addIrregularity = (irregularityTypeId: number) => {
    if (irrScores.some((s) => s.irregularityTypeId === irregularityTypeId)) return
    patchBlock((b) => ({
      ...b,
      esg: {
        ...b.esg,
        irregularityScores: [
          ...b.esg.irregularityScores,
          { irregularityTypeId, score: 0 },
        ],
      },
    }))
  }

  const removeIrregularity = (irregularityTypeId: number) => {
    patchBlock((b) => ({
      ...b,
      esg: {
        ...b.esg,
        irregularityScores: b.esg.irregularityScores.filter(
          (s) => s.irregularityTypeId !== irregularityTypeId,
        ),
      },
    }))
  }

  const configuredIds = new Set(irrScores.map((s) => s.irregularityTypeId))
  const availableIrr = (irregularityCatalog.data ?? []).filter((t) => !configuredIds.has(t.id))

  return (
    <section className="space-y-4">
      <SectionTitle
        title="ESG"
        hint={`${hints.esg}\n\nReforestation and native forest use the latest scope season. Irregularities use ESG irregularity KPI rows for that season.`}
      />
      <RuleSentence>
        Each percentage of Reforestation =
        <InlineDecimal
          value={e.reforestationScorePerPercentualPoint}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              esg: { ...b.esg, reforestationScorePerPercentualPoint: v },
            }))
          }
        />
        <span className="text-ink-muted">[MAXIMUM SCORE =</span>
        <InlineInt
          value={e.reforestationMaximumScore}
          onChange={(v) =>
            patchBlock((b) => ({ ...b, esg: { ...b.esg, reforestationMaximumScore: v } }))
          }
        />
        <span className="text-ink-muted">]</span>
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        Each percentage of Native Forest =
        <InlineInt
          value={e.nativeForestScorePerPercentualPoint}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              esg: { ...b.esg, nativeForestScorePerPercentualPoint: v },
            }))
          }
        />
        <span className="text-ink-muted">[MAXIMUM SCORE =</span>
        <InlineInt
          value={e.nativeForestMaximumScore}
          onChange={(v) =>
            patchBlock((b) => ({ ...b, esg: { ...b.esg, nativeForestMaximumScore: v } }))
          }
        />
        <span className="text-ink-muted">]</span>
        <PointsLabel />
      </RuleSentence>

      <h3 className="text-xs font-semibold uppercase tracking-wide text-ink-faint">
        Irregularities
      </h3>
      {irrScores.map((row) => {
        const name =
          irregularityCatalog.data?.find((t) => t.id === row.irregularityTypeId)?.name ??
          `Type #${row.irregularityTypeId}`
        return (
          <div key={row.irregularityTypeId} className="flex flex-wrap items-center gap-2">
            <RuleSentence>
              Has {name} =
              <InlineInt
                value={row.score}
                onChange={(v) => upsertIrregularity(row.irregularityTypeId, v)}
              />
              <PointsLabel />
            </RuleSentence>
            <button
              type="button"
              className="text-xs text-amber-700 hover:underline"
              onClick={() => removeIrregularity(row.irregularityTypeId)}
            >
              Remove
            </button>
          </div>
        )
      })}
      {availableIrr.length > 0 && (
        <label className="flex flex-wrap items-center gap-2 text-sm">
          <span className="text-ink-muted">Add irregularity type</span>
          <select
            className="rounded-lg border border-black/10 px-2 py-1"
            defaultValue=""
            onChange={(ev) => {
              const id = Number(ev.target.value)
              ev.target.value = ''
              if (Number.isFinite(id)) addIrregularity(id)
            }}
          >
            <option value="">Select…</option>
            {availableIrr.map((t) => (
              <option key={t.id} value={t.id}>
                {t.name}
              </option>
            ))}
          </select>
        </label>
      )}
    </section>
  )
}
