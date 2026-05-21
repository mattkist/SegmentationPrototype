import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { InlineInt, PointsLabel, RuleBlock, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

export function FinancialEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const f = block.financial

  return (
    <section className="space-y-6">
      <SectionTitle
        title="Financial"
        hint={`${hints.financialSf}\n\nSimulation uses only the latest crop season in the scope list.`}
      />

      <RuleSentence>
        If Have debt from last season =
        <InlineInt
          value={f.debtScore}
          onChange={(v) => patchBlock((b) => ({ ...b, financial: { ...b.financial, debtScore: v } }))}
        />
        <PointsLabel />
      </RuleSentence>

      <section className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-ink">Self-funding ranges</p>
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              patchBlock((b) => ({
                ...b,
                financial: {
                  ...b.financial,
                  selfFundingRanges: [
                    ...b.financial.selfFundingRanges,
                    { minimum: 0, maximum: 100, cropSeasonAmount: 1, score: 0 },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        {f.selfFundingRanges.map((r, i) => (
          <RuleBlock
            key={i}
            onRemove={() =>
              patchBlock((b) => ({
                ...b,
                financial: {
                  ...b.financial,
                  selfFundingRanges: b.financial.selfFundingRanges.filter((_, j) => j !== i),
                },
              }))
            }
          >
            <RuleSentence>
              Self Funding between
              <InlineInt
                value={r.minimum}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.financial.selfFundingRanges]
                    arr[i] = { ...arr[i], minimum: v }
                    return { ...b, financial: { ...b.financial, selfFundingRanges: arr } }
                  })
                }
              />
              % and
              <InlineInt
                value={r.maximum}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.financial.selfFundingRanges]
                    arr[i] = { ...arr[i], maximum: v }
                    return { ...b, financial: { ...b.financial, selfFundingRanges: arr } }
                  })
                }
              />
              % =
              <InlineInt
                value={r.score}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.financial.selfFundingRanges]
                    arr[i] = { ...arr[i], score: v }
                    return { ...b, financial: { ...b.financial, selfFundingRanges: arr } }
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
