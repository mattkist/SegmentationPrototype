import type { Dispatch, SetStateAction } from 'react'
import type { QualityIqsRangeDto, SaveSegmentationConfigurationDto } from '../../api/types'
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

export function QualityEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const q = draft.quality
  const patchRow = (i: number, part: Partial<QualityIqsRangeDto>) => {
    setDraft((d) => ({
      ...d,
      quality: {
        ...d.quality,
        iqsRanges: d.quality.iqsRanges.map((x, j) => (j === i ? { ...x, ...part } : x)),
      },
    }))
  }

  return (
    <div className="space-y-6">
      <SectionTitle title="Quality" hint={hints.qualityIqs} />

      <div className="space-y-3 rounded-xl border border-black/5 bg-surface-card/50 p-4">
        <p className="text-xs font-medium uppercase tracking-wide text-ink-muted">NTRM & mixture (anchors)</p>
        <div className="grid gap-3 sm:grid-cols-3">
          <IntInput
            label="NTRM crop season amount"
            value={q.ntrmCropSeasonAmount}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, ntrmCropSeasonAmount: v } }))}
          />
          <IntInput
            label="NTRM crop season start"
            value={q.ntrmCropSeasonStart}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, ntrmCropSeasonStart: v } }))}
          />
          <IntInput
            label="NTRM score"
            value={q.ntrmScore}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, ntrmScore: v } }))}
          />
        </div>
        <div className="grid gap-3 sm:grid-cols-3">
          <IntInput
            label="Mixture crop season amount"
            value={q.mixtureCropSeasonAmount}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, mixtureCropSeasonAmount: v } }))}
          />
          <IntInput
            label="Mixture crop season start"
            value={q.mixtureCropSeasonStart}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, mixtureCropSeasonStart: v } }))}
          />
          <IntInput
            label="Mixture score"
            value={q.mixtureScore}
            onChange={(v) => setDraft((d) => ({ ...d, quality: { ...d.quality, mixtureScore: v } }))}
          />
        </div>
      </div>

      <div>
        <div className="mb-2 flex justify-between">
          <FieldLabel label="IQS ranges" hint={hints.qualityIqs} />
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              setDraft((d) => ({
                ...d,
                quality: {
                  ...d.quality,
                  iqsRanges: [
                    ...d.quality.iqsRanges,
                    {
                      minimum: 0,
                      maximum: 100,
                      cropSeasonAmount: 1,
                      cropSeasonStart: 2026,
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
          {q.iqsRanges.map((r, i) => (
            <div key={i} className="grid gap-2 rounded-lg border border-black/5 p-3 sm:grid-cols-3 lg:grid-cols-6">
              <IntInput label="Min IQS" value={r.minimum} onChange={(v) => patchRow(i, { minimum: v })} />
              <IntInput label="Max IQS" value={r.maximum} onChange={(v) => patchRow(i, { maximum: v })} />
              <IntInput
                label="Crop season amount"
                value={r.cropSeasonAmount}
                onChange={(v) => patchRow(i, { cropSeasonAmount: v })}
              />
              <IntInput
                label="Crop season start"
                value={r.cropSeasonStart}
                onChange={(v) => patchRow(i, { cropSeasonStart: v })}
              />
              <IntInput label="Score" value={r.score} onChange={(v) => patchRow(i, { score: v })} />
              <label className="text-xs sm:col-span-2 lg:col-span-6">
                <FieldLabel label="Skipped season ids" hint="Comma-separated crop season ids skipped in the window." />
                <input
                  className="mt-1 w-full rounded border border-black/10 px-2 py-1 font-mono text-xs focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20"
                  value={r.skippedCropSeasonIds.join(', ')}
                  onChange={(e) => patchRow(i, { skippedCropSeasonIds: parseSkipped(e.target.value) })}
                />
              </label>
              <button
                type="button"
                className="text-xs font-medium text-red-700 hover:underline"
                onClick={() =>
                  setDraft((d) => ({
                    ...d,
                    quality: { ...d.quality, iqsRanges: d.quality.iqsRanges.filter((_, j) => j !== i) },
                  }))
                }
              >
                Remove
              </button>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
