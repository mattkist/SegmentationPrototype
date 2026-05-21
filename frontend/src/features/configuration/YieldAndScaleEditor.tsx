import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { InlineInt, InlineModuleHa, PointsLabel, RuleBlock, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

export function YieldAndScaleEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const ys = block.yieldAndScale

  return (
    <section className="space-y-6">
      <SectionTitle
        title="Yield & Scale"
        hint={`${hints.yieldAndScale}\n\nUses all selected simulation scope seasons (like Loyalty). Average yield and module (scale) are computed across seasons where both KPIs exist.`}
      />

      <section className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-ink">Combined ranges</p>
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              patchBlock((b) => ({
                ...b,
                yieldAndScale: {
                  ...b.yieldAndScale,
                  ranges: [
                    ...b.yieldAndScale.ranges,
                    {
                      yieldAndScaleCropSeasonAmount: 1,
                      minimumYield: 0,
                      maximumYield: 999999,
                      minimumModule: 0,
                      maximumModule: 999999,
                      score: 0,
                    },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        {ys.ranges.length === 0 && (
          <p className="text-xs text-ink-muted">
            No ranges configured. Set relevance to 0% in KPI caps if you use separate Yield and Scale only.
          </p>
        )}
        {ys.ranges.map((r, i) => (
          <RuleBlock
            key={i}
            onRemove={() =>
              patchBlock((b) => ({
                ...b,
                yieldAndScale: {
                  ...b.yieldAndScale,
                  ranges: b.yieldAndScale.ranges.filter((_, j) => j !== i),
                },
              }))
            }
          >
            <RuleSentence>
              Planting for company in
              <InlineInt
                value={r.yieldAndScaleCropSeasonAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], yieldAndScaleCropSeasonAmount: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
                  })
                }
              />
              Crop Seasons
            </RuleSentence>
            <RuleSentence className="font-medium">AND</RuleSentence>
            <RuleSentence>
              Average Yield BETWEEN
              <InlineInt
                value={r.minimumYield}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], minimumYield: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
                  })
                }
              />
              and
              <InlineInt
                value={r.maximumYield}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], maximumYield: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
                  })
                }
              />
            </RuleSentence>
            <RuleSentence>
              AND Module between
              <InlineModuleHa
                value={r.minimumModule}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], minimumModule: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
                  })
                }
              />
              ha and
              <InlineModuleHa
                value={r.maximumModule}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], maximumModule: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
                  })
                }
              />
              ha =
              <InlineInt
                value={r.score}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.yieldAndScale.ranges]
                    arr[i] = { ...arr[i], score: v }
                    return { ...b, yieldAndScale: { ...b.yieldAndScale, ranges: arr } }
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
