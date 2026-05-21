import type { SaveSegmentationConfigurationDto } from '../../api/types'
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
  const t = block.technology
  return (
    <section className="space-y-4">
      <SectionTitle
        title="Technology"
        hint={`${hints.technology}\n\nSimulation uses only the latest crop season in the scope list.`}
      />
      <RuleSentence>
        If Has Large Base Ridge With Mulch =
        <InlineInt
          value={t.hasLargeBaseRidgeWithMulchScore}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              technology: { ...b.technology, hasLargeBaseRidgeWithMulchScore: v },
            }))
          }
        />
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        If Has Broad Grate Furnace =
        <InlineInt
          value={t.hasBroadGrateFurnaceScore}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              technology: { ...b.technology, hasBroadGrateFurnaceScore: v },
            }))
          }
        />
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        If Has Technology Package Adherence =
        <InlineInt
          value={t.hasTechnologyPackageAdherenceScore}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              technology: { ...b.technology, hasTechnologyPackageAdherenceScore: v },
            }))
          }
        />
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        If Has Standard Barn =
        <InlineInt
          value={t.hasStandardBarnScore}
          onChange={(v) =>
            patchBlock((b) => ({
              ...b,
              technology: { ...b.technology, hasStandardBarnScore: v },
            }))
          }
        />
        <PointsLabel />
      </RuleSentence>
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
  const e = block.esg
  return (
    <section className="space-y-4">
      <SectionTitle
        title="ESG"
        hint={`${hints.esg}\n\nSimulation uses only the latest crop season in the scope list.`}
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
      <RuleSentence>
        Has Minor Irregularity =
        <InlineInt
          value={e.minorIrregularityScore}
          onChange={(v) =>
            patchBlock((b) => ({ ...b, esg: { ...b.esg, minorIrregularityScore: v } }))
          }
        />
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        Has Major Irregularity =
        <InlineInt
          value={e.majorIrregularityScore}
          onChange={(v) =>
            patchBlock((b) => ({ ...b, esg: { ...b.esg, majorIrregularityScore: v } }))
          }
        />
        <PointsLabel />
      </RuleSentence>
    </section>
  )
}
