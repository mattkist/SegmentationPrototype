import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { InlineInt, PointsLabel, RuleBlock, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

export function QualityEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const q = block.quality

  return (
    <section className="space-y-6">
      <SectionTitle
        title="Quality"
        hint={`${hints.qualityIqs}\n\nSimulation uses only the latest (highest) crop season in the scope list for IQS, NTRM, and mixture.`}
      />

      <RuleSentence>
        If Had NTRM in past season =
        <InlineInt
          value={q.ntrmScore}
          onChange={(v) => patchBlock((b) => ({ ...b, quality: { ...b.quality, ntrmScore: v } }))}
        />
        <PointsLabel />
      </RuleSentence>
      <RuleSentence>
        If Had quality mixture, green leafs in past season =
        <InlineInt
          value={q.mixtureScore}
          onChange={(v) => patchBlock((b) => ({ ...b, quality: { ...b.quality, mixtureScore: v } }))}
        />
        <PointsLabel />
      </RuleSentence>

      <section className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-ink">IQS ranges</p>
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              patchBlock((b) => ({
                ...b,
                quality: {
                  ...b.quality,
                  iqsRanges: [
                    ...b.quality.iqsRanges,
                    { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 0 },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        {q.iqsRanges.map((r, i) => (
          <RuleBlock
            key={i}
            onRemove={() =>
              patchBlock((b) => ({
                ...b,
                quality: {
                  ...b.quality,
                  iqsRanges: b.quality.iqsRanges.filter((_, j) => j !== i),
                },
              }))
            }
          >
            <RuleSentence>
              IQS Between
              <InlineInt
                value={r.minimum}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.quality.iqsRanges]
                    arr[i] = { ...arr[i], minimum: v }
                    return { ...b, quality: { ...b.quality, iqsRanges: arr } }
                  })
                }
              />
              and
              <InlineInt
                value={r.maximum}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.quality.iqsRanges]
                    arr[i] = { ...arr[i], maximum: v }
                    return { ...b, quality: { ...b.quality, iqsRanges: arr } }
                  })
                }
              />
              =
              <InlineInt
                value={r.score}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.quality.iqsRanges]
                    arr[i] = { ...arr[i], score: v }
                    return { ...b, quality: { ...b.quality, iqsRanges: arr } }
                  })
                }
              />
              <PointsLabel />
            </RuleSentence>
          </RuleBlock>
        ))}
      </section>
    </section>
  )
}
