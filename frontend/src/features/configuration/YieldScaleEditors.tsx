import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto, YieldRangeDto } from '../../api/types'
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

export function YieldEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const y = draft.yield
  const patch = (i: number, part: Partial<YieldRangeDto>) => {
    setDraft((d) => ({
      ...d,
      yield: {
        ...d.yield,
        ranges: d.yield.ranges.map((x, j) => (j === i ? { ...x, ...part } : x)),
      },
    }))
  }
  return (
    <div className="space-y-4">
      <SectionTitle title="Yield" hint={hints.yieldScale} />
      <div className="flex justify-end">
        <button
          type="button"
          className="text-xs font-semibold text-leaf hover:underline"
          onClick={() =>
            setDraft((d) => ({
              ...d,
              yield: {
                ...d.yield,
                ranges: [
                  ...d.yield.ranges,
                  {
                    minimum: 0,
                    maximum: 9999,
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
          + Add yield range
        </button>
      </div>
      <div className="space-y-3">
        {y.ranges.map((r, i) => (
          <div key={i} className="grid gap-2 rounded-lg border border-black/5 p-3 sm:grid-cols-3 lg:grid-cols-6">
            <IntInput label="Min yield" value={r.minimum} onChange={(v) => patch(i, { minimum: v })} />
            <IntInput label="Max yield" value={r.maximum} onChange={(v) => patch(i, { maximum: v })} />
            <IntInput
              label="Crop season amount"
              value={r.cropSeasonAmount}
              onChange={(v) => patch(i, { cropSeasonAmount: v })}
            />
            <IntInput
              label="Crop season start"
              value={r.cropSeasonStart}
              onChange={(v) => patch(i, { cropSeasonStart: v })}
            />
            <IntInput label="Score" value={r.score} onChange={(v) => patch(i, { score: v })} />
            <label className="text-xs sm:col-span-2 lg:col-span-6">
              <FieldLabel label="Skipped season ids" hint="Comma-separated ids." />
              <input
                className="mt-1 w-full rounded border border-black/10 px-2 py-1 font-mono text-xs focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20"
                value={r.skippedCropSeasonIds.join(', ')}
                onChange={(e) => patch(i, { skippedCropSeasonIds: parseSkipped(e.target.value) })}
              />
            </label>
            <button
              type="button"
              className="text-xs font-medium text-red-700 hover:underline"
              onClick={() =>
                setDraft((d) => ({
                  ...d,
                  yield: { ...d.yield, ranges: d.yield.ranges.filter((_, j) => j !== i) },
                }))
              }
            >
              Remove
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}

export function ScaleEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const s = draft.scale
  const patch = (i: number, part: Partial<(typeof s.ranges)[0]>) => {
    setDraft((d) => ({
      ...d,
      scale: {
        ...d.scale,
        ranges: d.scale.ranges.map((x, j) => (j === i ? { ...x, ...part } : x)),
      },
    }))
  }
  return (
    <div className="space-y-4">
      <SectionTitle title="Scale" hint={hints.yieldScale} />
      <div className="flex justify-end">
        <button
          type="button"
          className="text-xs font-semibold text-leaf hover:underline"
          onClick={() =>
            setDraft((d) => ({
              ...d,
              scale: {
                ...d.scale,
                ranges: [
                  ...d.scale.ranges,
                  {
                    minimum: 0,
                    maximum: 9999,
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
          + Add scale range
        </button>
      </div>
      <div className="space-y-3">
        {s.ranges.map((r, i) => (
          <div key={i} className="grid gap-2 rounded-lg border border-black/5 p-3 sm:grid-cols-3 lg:grid-cols-6">
            <IntInput label="Min scale" value={r.minimum} onChange={(v) => patch(i, { minimum: v })} />
            <IntInput label="Max scale" value={r.maximum} onChange={(v) => patch(i, { maximum: v })} />
            <IntInput
              label="Crop season amount"
              value={r.cropSeasonAmount}
              onChange={(v) => patch(i, { cropSeasonAmount: v })}
            />
            <IntInput
              label="Crop season start"
              value={r.cropSeasonStart}
              onChange={(v) => patch(i, { cropSeasonStart: v })}
            />
            <IntInput label="Score" value={r.score} onChange={(v) => patch(i, { score: v })} />
            <label className="text-xs sm:col-span-2 lg:col-span-6">
              <FieldLabel label="Skipped season ids" hint="Comma-separated ids." />
              <input
                className="mt-1 w-full rounded border border-black/10 px-2 py-1 font-mono text-xs focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20"
                value={r.skippedCropSeasonIds.join(', ')}
                onChange={(e) => patch(i, { skippedCropSeasonIds: parseSkipped(e.target.value) })}
              />
            </label>
            <button
              type="button"
              className="text-xs font-medium text-red-700 hover:underline"
              onClick={() =>
                setDraft((d) => ({
                  ...d,
                  scale: { ...d.scale, ranges: d.scale.ranges.filter((_, j) => j !== i) },
                }))
              }
            >
              Remove
            </button>
          </div>
        ))}
      </div>
    </div>
  )
}
