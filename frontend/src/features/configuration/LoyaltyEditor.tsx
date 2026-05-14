import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { FieldLabel, SectionTitle } from '../../components/Hint'
import { IntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

function parseSkipped(raw: string): number[] {
  return raw
    .split(/[,;\s]+/)
    .map((s) => s.trim())
    .filter(Boolean)
    .map((s) => Number(s))
    .filter((n) => Number.isFinite(n))
}

export function LoyaltyEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const l = draft.loyalty
  return (
    <div className="space-y-6">
      <SectionTitle title="Loyalty" hint={`${hints.loyaltyHistorical}\n\n${hints.loyaltySeasonQty}`} />

      <div>
        <div className="mb-2 flex items-center justify-between">
          <FieldLabel label="Historical volume ranges" hint={hints.loyaltyHistorical} />
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              setDraft((d) => ({
                ...d,
                loyalty: {
                  ...d.loyalty,
                  historicalVolumeRanges: [
                    ...d.loyalty.historicalVolumeRanges,
                    { minimumDeliveryAmount: 0, maximumDeliveryAmount: 100, score: 0 },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        <div className="space-y-2">
          {l.historicalVolumeRanges.map((r, i) => (
            <div
              key={i}
              className="flex flex-wrap items-end gap-3 rounded-lg border border-black/5 bg-surface-card/50 p-3"
            >
              <IntInput
                className="min-w-[7rem] flex-1"
                label="Min delivery %"
                value={r.minimumDeliveryAmount}
                onChange={(v) =>
                  setDraft((d) => ({
                    ...d,
                    loyalty: {
                      ...d.loyalty,
                      historicalVolumeRanges: d.loyalty.historicalVolumeRanges.map((x, j) =>
                        j === i ? { ...x, minimumDeliveryAmount: v } : x,
                      ),
                    },
                  }))
                }
              />
              <IntInput
                className="min-w-[7rem] flex-1"
                label="Max delivery %"
                value={r.maximumDeliveryAmount}
                onChange={(v) =>
                  setDraft((d) => ({
                    ...d,
                    loyalty: {
                      ...d.loyalty,
                      historicalVolumeRanges: d.loyalty.historicalVolumeRanges.map((x, j) =>
                        j === i ? { ...x, maximumDeliveryAmount: v } : x,
                      ),
                    },
                  }))
                }
              />
              <IntInput
                className="min-w-[6rem] flex-1"
                label="Score"
                value={r.score}
                onChange={(v) =>
                  setDraft((d) => ({
                    ...d,
                    loyalty: {
                      ...d.loyalty,
                      historicalVolumeRanges: d.loyalty.historicalVolumeRanges.map((x, j) =>
                        j === i ? { ...x, score: v } : x,
                      ),
                    },
                  }))
                }
              />
              <button
                type="button"
                className="shrink-0 text-xs font-medium text-red-700 hover:underline"
                onClick={() =>
                  setDraft((d) => ({
                    ...d,
                    loyalty: {
                      ...d.loyalty,
                      historicalVolumeRanges: d.loyalty.historicalVolumeRanges.filter((_, j) => j !== i),
                    },
                  }))
                }
              >
                Remove
              </button>
            </div>
          ))}
        </div>
      </div>

      <div>
        <div className="mb-2 flex items-center justify-between">
          <FieldLabel label="Season quantity ranges" hint={hints.loyaltySeasonQty} />
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              setDraft((d) => ({
                ...d,
                loyalty: {
                  ...d.loyalty,
                  seasonQuantityRanges: [
                    ...d.loyalty.seasonQuantityRanges,
                    {
                      plantingCropSeasonAmount: 1,
                      cropSeasonStart: 2026,
                      minimumDeliveryAmount: 0,
                      maximumDeliveryAmount: 100,
                      deliveryCropSeasonAmount: 1,
                      score: 0,
                      skippedCropSeasonIds: [],
                    },
                  ],
                },
              }))
            }
          >
            + Add range
          </button>
        </div>
        <div className="space-y-3">
          {l.seasonQuantityRanges.map((r, i) => (
            <div key={i} className="space-y-3 rounded-xl border border-black/5 bg-surface-card/50 p-4">
              <div className="grid gap-3 sm:grid-cols-2">
                <IntInput
                  label="Planting crop season amount"
                  value={r.plantingCropSeasonAmount}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, plantingCropSeasonAmount: v } : x,
                        ),
                      },
                    }))
                  }
                />
                <IntInput
                  label="Crop season start"
                  value={r.cropSeasonStart}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, cropSeasonStart: v } : x,
                        ),
                      },
                    }))
                  }
                />
              </div>
              <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
                <IntInput
                  label="Min delivery %"
                  value={r.minimumDeliveryAmount}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, minimumDeliveryAmount: v } : x,
                        ),
                      },
                    }))
                  }
                />
                <IntInput
                  label="Max delivery %"
                  value={r.maximumDeliveryAmount}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, maximumDeliveryAmount: v } : x,
                        ),
                      },
                    }))
                  }
                />
                <IntInput
                  label="Delivery crop season amount"
                  value={r.deliveryCropSeasonAmount}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, deliveryCropSeasonAmount: v } : x,
                        ),
                      },
                    }))
                  }
                />
                <div className="text-xs sm:col-span-2 lg:col-span-4">
                  <FieldLabel
                    label="Skipped crop season ids"
                    hint="Comma-separated season ids excluded from consecutive window checks."
                  />
                  <input
                    className="mt-0.5 w-full rounded border border-black/10 px-2 py-1 font-mono text-sm focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20"
                    value={r.skippedCropSeasonIds.join(', ')}
                    onChange={(e) =>
                      setDraft((d) => ({
                        ...d,
                        loyalty: {
                          ...d.loyalty,
                          seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                            j === i ? { ...x, skippedCropSeasonIds: parseSkipped(e.target.value) } : x,
                          ),
                        },
                      }))
                    }
                  />
                </div>
              </div>
              <div className="flex flex-wrap items-end justify-between gap-3 border-t border-black/5 pt-3">
                <IntInput
                  className="max-w-[12rem]"
                  label="Score"
                  value={r.score}
                  onChange={(v) =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.map((x, j) =>
                          j === i ? { ...x, score: v } : x,
                        ),
                      },
                    }))
                  }
                />
                <button
                  type="button"
                  className="text-xs font-medium text-red-700 hover:underline"
                  onClick={() =>
                    setDraft((d) => ({
                      ...d,
                      loyalty: {
                        ...d.loyalty,
                        seasonQuantityRanges: d.loyalty.seasonQuantityRanges.filter((_, j) => j !== i),
                      },
                    }))
                  }
                >
                  Remove row
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
