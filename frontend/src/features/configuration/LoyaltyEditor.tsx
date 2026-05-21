import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { InlineInt, PointsLabel, RuleBlock, RuleSentence } from '../../components/RuleSentence'
import { SectionTitle } from '../../components/Hint'
import { hints } from '../../hints/en'
import { useCultureTypeEditor, type SetDraft } from './cultureTypeEditorUtils'

export function LoyaltyEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const { block, patchBlock } = useCultureTypeEditor(draft, setDraft, cultureTypeCode)
  const l = block.loyalty

  return (
    <section className="space-y-8">
      <SectionTitle title="Loyalty" hint={`${hints.loyaltyHistorical}\n\n${hints.loyaltySeasonQty}`} />

      <section className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-ink">Historical volume ranges</p>
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              patchBlock((b) => ({
                ...b,
                loyalty: {
                  ...b.loyalty,
                  historicalVolumeRanges: [
                    ...b.loyalty.historicalVolumeRanges,
                    { minimumDeliveryAmount: 0, maximumDeliveryAmount: 100, score: 0 },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        {l.historicalVolumeRanges.map((r, i) => (
          <RuleBlock
            key={i}
            onRemove={() =>
              patchBlock((b) => ({
                ...b,
                loyalty: {
                  ...b.loyalty,
                  historicalVolumeRanges: b.loyalty.historicalVolumeRanges.filter((_, j) => j !== i),
                },
              }))
            }
          >
            <RuleSentence>
              Historical Delivery amount between
              <InlineInt
                value={r.minimumDeliveryAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.historicalVolumeRanges]
                    arr[i] = { ...arr[i], minimumDeliveryAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, historicalVolumeRanges: arr } }
                  })
                }
              />
              –
              <InlineInt
                value={r.maximumDeliveryAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.historicalVolumeRanges]
                    arr[i] = { ...arr[i], maximumDeliveryAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, historicalVolumeRanges: arr } }
                  })
                }
              />
              % =
              <InlineInt
                value={r.score}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.historicalVolumeRanges]
                    arr[i] = { ...arr[i], score: v }
                    return { ...b, loyalty: { ...b.loyalty, historicalVolumeRanges: arr } }
                  })
                }
              />
              <PointsLabel />
            </RuleSentence>
          </RuleBlock>
        ))}
      </section>

      <section className="space-y-3">
        <div className="flex items-center justify-between gap-2">
          <p className="text-sm font-medium text-ink">Season quantity rules</p>
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              patchBlock((b) => ({
                ...b,
                loyalty: {
                  ...b.loyalty,
                  seasonQuantityRanges: [
                    ...b.loyalty.seasonQuantityRanges,
                    {
                      plantingCropSeasonAmount: 1,
                      minimumDeliveryAmount: 0,
                      maximumDeliveryAmount: 100,
                      deliveryCropSeasonAmount: 1,
                      score: 0,
                    },
                  ],
                },
              }))
            }
          >
            + Add rule
          </button>
        </div>
        {l.seasonQuantityRanges.map((r, i) => (
          <RuleBlock
            key={i}
            onRemove={() =>
              patchBlock((b) => ({
                ...b,
                loyalty: {
                  ...b.loyalty,
                  seasonQuantityRanges: b.loyalty.seasonQuantityRanges.filter((_, j) => j !== i),
                },
              }))
            }
          >
            <RuleSentence>
              Planting for company in at least
              <InlineInt
                value={r.plantingCropSeasonAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.seasonQuantityRanges]
                    arr[i] = { ...arr[i], plantingCropSeasonAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, seasonQuantityRanges: arr } }
                  })
                }
              />
              crop season
            </RuleSentence>
            <RuleSentence className="font-medium">AND</RuleSentence>
            <RuleSentence>
              Delivery Amount Between
              <InlineInt
                value={r.minimumDeliveryAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.seasonQuantityRanges]
                    arr[i] = { ...arr[i], minimumDeliveryAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, seasonQuantityRanges: arr } }
                  })
                }
              />
              –
              <InlineInt
                value={r.maximumDeliveryAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.seasonQuantityRanges]
                    arr[i] = { ...arr[i], maximumDeliveryAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, seasonQuantityRanges: arr } }
                  })
                }
              />
              % in
              <InlineInt
                value={r.deliveryCropSeasonAmount}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.seasonQuantityRanges]
                    arr[i] = { ...arr[i], deliveryCropSeasonAmount: v }
                    return { ...b, loyalty: { ...b.loyalty, seasonQuantityRanges: arr } }
                  })
                }
              />
              Crop Seasons =
              <InlineInt
                value={r.score}
                onChange={(v) =>
                  patchBlock((b) => {
                    const arr = [...b.loyalty.seasonQuantityRanges]
                    arr[i] = { ...arr[i], score: v }
                    return { ...b, loyalty: { ...b.loyalty, seasonQuantityRanges: arr } }
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
