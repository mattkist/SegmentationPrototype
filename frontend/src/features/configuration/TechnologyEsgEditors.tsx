import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { SectionTitle } from '../../components/Hint'
import { DecimalInput, IntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function TechnologyEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const t = draft.technology
  return (
    <div className="space-y-4">
      <SectionTitle title="Technology" hint={hints.technology} />
      <div className="space-y-3 rounded-xl border border-black/5 bg-surface-card/50 p-4">
        <div className="grid gap-3 sm:grid-cols-2">
          <IntInput
            label="Mulch — from season"
            value={t.hasLargeBaseRidgeWithMulchCropSeason}
            onChange={(v) =>
              setDraft((d) => ({ ...d, technology: { ...d.technology, hasLargeBaseRidgeWithMulchCropSeason: v } }))
            }
          />
          <IntInput
            label="Mulch score"
            value={t.hasLargeBaseRidgeWithMulchScore}
            onChange={(v) =>
              setDraft((d) => ({ ...d, technology: { ...d.technology, hasLargeBaseRidgeWithMulchScore: v } }))
            }
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <IntInput
            label="Furnace — from season"
            value={t.hasBroadGrateFurnaceCropSeason}
            onChange={(v) =>
              setDraft((d) => ({ ...d, technology: { ...d.technology, hasBroadGrateFurnaceCropSeason: v } }))
            }
          />
          <IntInput
            label="Furnace score"
            value={t.hasBroadGrateFurnaceScore}
            onChange={(v) => setDraft((d) => ({ ...d, technology: { ...d.technology, hasBroadGrateFurnaceScore: v } }))}
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <IntInput
            label="Package — from season"
            value={t.hasTechnologyPackageAdherenceCropSeason}
            onChange={(v) =>
              setDraft((d) => ({
                ...d,
                technology: { ...d.technology, hasTechnologyPackageAdherenceCropSeason: v },
              }))
            }
          />
          <IntInput
            label="Package score"
            value={t.hasTechnologyPackageAdherenceScore}
            onChange={(v) =>
              setDraft((d) => ({ ...d, technology: { ...d.technology, hasTechnologyPackageAdherenceScore: v } }))
            }
          />
        </div>
      </div>
    </div>
  )
}

export function EsgEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const e = draft.esg
  return (
    <div className="space-y-4">
      <SectionTitle title="ESG" hint={hints.esg} />
      <div className="space-y-3 rounded-xl border border-black/5 bg-surface-card/50 p-4">
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <IntInput
            label="Reforestation — from season"
            value={e.reforestationCropSeason}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, reforestationCropSeason: v } }))}
          />
          <DecimalInput
            label="Reforestation pts / %"
            value={e.reforestationScorePerPercentualPoint}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, reforestationScorePerPercentualPoint: v } }))}
          />
          <IntInput
            label="Reforestation max score"
            value={e.reforestationMaximumScore}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, reforestationMaximumScore: v } }))}
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
          <IntInput
            label="Native forest — from season"
            value={e.nativeForestCropSeason}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, nativeForestCropSeason: v } }))}
          />
          <DecimalInput
            label="Native pts / %"
            value={e.nativeForestScorePerPercentualPoint}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, nativeForestScorePerPercentualPoint: v } }))}
          />
          <IntInput
            label="Native max score"
            value={e.nativeForestMaximumScore}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, nativeForestMaximumScore: v } }))}
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <IntInput
            label="Minor irregularity — from season"
            value={e.minorIrregularityCropSeason}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, minorIrregularityCropSeason: v } }))}
          />
          <IntInput
            label="Minor irregularity score"
            value={e.minorIrregularityScore}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, minorIrregularityScore: v } }))}
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-2">
          <IntInput
            label="Major irregularity — from season"
            value={e.majorIrregularityCropSeason}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, majorIrregularityCropSeason: v } }))}
          />
          <IntInput
            label="Major irregularity score"
            value={e.majorIrregularityScore}
            onChange={(v) => setDraft((d) => ({ ...d, esg: { ...d.esg, majorIrregularityScore: v } }))}
          />
        </div>
      </div>
    </div>
  )
}
