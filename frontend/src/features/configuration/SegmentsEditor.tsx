import type { Dispatch, SetStateAction } from 'react'
import type {
  SaveSegmentationConfigurationDto,
  SegmentationSegmentDto,
} from '../../api/types'
import { FieldLabel } from '../../components/Hint'
import { IntInput, NullableIntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function SegmentsEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const update = (i: number, part: Partial<SegmentationSegmentDto>) => {
    setDraft((d) => ({
      ...d,
      segments: d.segments.map((s, j) => (j === i ? { ...s, ...part } : s)),
    }))
  }

  const add = () => {
    setDraft((d) => ({
      ...d,
      segments: [
        ...d.segments,
        {
          id: undefined,
          segmentName: 'New segment',
          rangeMin: null,
          onlyExclusiveFarmer: false,
          bankDepositDiscount: 0,
          tobaccoDiscount: 0,
        },
      ],
    }))
  }

  const remove = (i: number) => {
    setDraft((d) => ({
      ...d,
      segments: d.segments.filter((_, j) => j !== i),
    }))
  }

  return (
    <div className="space-y-3">
      <div className="flex items-center justify-between gap-2">
        <FieldLabel label="Segments" hint={hints.configSegments} />
        <button
          type="button"
          onClick={add}
          className="rounded-lg bg-leaf px-3 py-1.5 text-xs font-semibold text-white hover:bg-leaf-hover"
        >
          Add segment
        </button>
      </div>
      <div className="space-y-3">
        {draft.segments.map((s, i) => (
          <div
            key={i}
            className="grid gap-3 rounded-xl border border-black/5 bg-surface-muted/40 p-4 sm:grid-cols-2 lg:grid-cols-3"
          >
            <Field
              label="Name"
              hint="Displayed tier name (e.g. Diamond, Gold)."
              value={s.segmentName}
              onChange={(v) => update(i, { segmentName: v })}
            />
            <div>
              <FieldLabel
                label="Range min (total score)"
                hint="Minimum total simulation score to qualify; may be negative. Leave empty for catch-all after thresholds."
              />
              <NullableIntInput
                value={s.rangeMin}
                onChange={(v) => update(i, { rangeMin: v })}
                className="mt-1"
              />
            </div>
            <label className="flex items-center gap-2 text-sm">
              <input
                type="checkbox"
                checked={s.onlyExclusiveFarmer}
                onChange={(e) => update(i, { onlyExclusiveFarmer: e.target.checked })}
              />
              <span className="text-ink-muted">Only exclusive farmer</span>
            </label>
            <div>
              <FieldLabel
                label="Bank deposit discount"
                hint="Commercial parameter stored with the segment (integer units in the prototype)."
              />
              <IntInput
                value={s.bankDepositDiscount}
                onChange={(v) => update(i, { bankDepositDiscount: v })}
                className="mt-1"
              />
            </div>
            <div>
              <FieldLabel label="Tobacco discount" hint="Commercial parameter stored with the segment." />
              <IntInput
                value={s.tobaccoDiscount}
                onChange={(v) => update(i, { tobaccoDiscount: v })}
                className="mt-1"
              />
            </div>
            <div className="flex items-end">
              <button
                type="button"
                onClick={() => remove(i)}
                className="text-sm font-medium text-red-700 hover:underline"
              >
                Remove
              </button>
            </div>
            {s.id && (
              <p className="text-xs text-ink-faint sm:col-span-2 lg:col-span-3">
                Segment id (keep on save): <span className="font-mono">{s.id}</span>
              </p>
            )}
          </div>
        ))}
      </div>
    </div>
  )
}

function Field({
  label,
  hint,
  value,
  onChange,
}: {
  label: string
  hint: string
  value: string
  onChange: (v: string) => void
}) {
  return (
    <label className="block text-sm">
      <FieldLabel label={label} hint={hint} />
      <input
        className="mt-1 w-full rounded-lg border border-black/10 px-2 py-1.5 text-sm focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20"
        value={value}
        onChange={(e) => onChange(e.target.value)}
      />
    </label>
  )
}
