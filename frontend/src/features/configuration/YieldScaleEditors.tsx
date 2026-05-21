import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { InlineInt, PointsLabel, RuleBlock, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

function RangeList({
  title,
  hint,
  labelBetween,
  ranges,
  onAdd,
  onPatch,
  onRemove,
}: {
  title: string
  hint: string
  labelBetween: string
  ranges: { minimum: number; maximum: number; cropSeasonAmount: number; score: number }[]
  onAdd: () => void
  onPatch: (i: number, part: Partial<{ minimum: number; maximum: number; score: number }>) => void
  onRemove: (i: number) => void
}) {
  return (
    <section className="space-y-3">
      <SectionTitle title={title} hint={hint} />
      <div className="flex items-center justify-end">
        <button type="button" className="text-xs font-semibold text-leaf hover:underline" onClick={onAdd}>
          + Add range
        </button>
      </div>
      {ranges.map((r, i) => (
        <RuleBlock key={i} onRemove={() => onRemove(i)}>
          <RuleSentence>
            {labelBetween}
            <InlineInt value={r.minimum} onChange={(v) => onPatch(i, { minimum: v })} />
            AND
            <InlineInt value={r.maximum} onChange={(v) => onPatch(i, { maximum: v })} />
            =
            <InlineInt value={r.score} onChange={(v) => onPatch(i, { score: v })} />
            <PointsLabel />
          </RuleSentence>
        </RuleBlock>
      ))}
    </section>
  )
}

export function YieldEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  return (
    <RangeList
      title="Yield"
      hint={`${hints.yieldScale}\n\nSimulation uses only the latest crop season in the scope list.`}
      labelBetween="Yield BETWEEN"
      ranges={block.yield.ranges}
      onAdd={() =>
        patchBlock((b) => ({
          ...b,
          yield: {
            ...b.yield,
            ranges: [...b.yield.ranges, { minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 0 }],
          },
        }))
      }
      onPatch={(i, part) =>
        patchBlock((b) => {
          const arr = [...b.yield.ranges]
          arr[i] = { ...arr[i], ...part }
          return { ...b, yield: { ...b.yield, ranges: arr } }
        })
      }
      onRemove={(i) =>
        patchBlock((b) => ({
          ...b,
          yield: { ...b.yield, ranges: b.yield.ranges.filter((_, j) => j !== i) },
        }))
      }
    />
  )
}

export function ScaleEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  return (
    <RangeList
      title="Scale"
      hint={`${hints.yieldScale}\n\nSimulation uses only the latest crop season in the scope list.`}
      labelBetween="Area BETWEEN"
      ranges={block.scale.ranges}
      onAdd={() =>
        patchBlock((b) => ({
          ...b,
          scale: {
            ...b.scale,
            ranges: [...b.scale.ranges, { minimum: 0, maximum: 999999, cropSeasonAmount: 1, score: 0 }],
          },
        }))
      }
      onPatch={(i, part) =>
        patchBlock((b) => {
          const arr = [...b.scale.ranges]
          arr[i] = { ...arr[i], ...part }
          return { ...b, scale: { ...b.scale, ranges: arr } }
        })
      }
      onRemove={(i) =>
        patchBlock((b) => ({
          ...b,
          scale: { ...b.scale, ranges: b.scale.ranges.filter((_, j) => j !== i) },
        }))
      }
    />
  )
}
