import type { Dispatch, SetStateAction } from 'react'
import type {
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
} from '../../api/types'
import { patchCultureTypeBlock } from '../../domain/cultureTypeDraft'
import { NullableIntInput } from '../../components/NumericInputs'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function CultureTypeSettingsEditor({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const block = draft.cultureTypes.find((c) => c.cultureTypeCode === cultureTypeCode)
  if (!block) return null

  const patchBlock = (fn: (b: CultureTypeConfigurationWriteDto) => CultureTypeConfigurationWriteDto) => {
    setDraft((d) => patchCultureTypeBlock(d, cultureTypeCode, fn))
  }

  return (
    <section className="space-y-3">
      <h3 className="text-sm font-semibold text-ink">Segment score thresholds</h3>
      <p className="text-xs text-ink-muted">
        Minimum total score per segment for culture type {cultureTypeCode}.
      </p>
      <div className="space-y-2">
        {block.segmentThresholds.map((t, i) => (
          <div
            key={t.segmentName}
            className="flex flex-wrap items-center gap-3 rounded-lg border border-black/5 bg-surface-muted/40 p-3"
          >
            <span className="min-w-[6rem] text-sm font-medium text-ink">{t.segmentName}</span>
            <div className="flex flex-wrap items-center gap-2 text-sm text-ink-muted">
              <span>Range min</span>
              <NullableIntInput
                value={t.rangeMin}
                onChange={(v) =>
                  patchBlock((b) => ({
                    ...b,
                    segmentThresholds: b.segmentThresholds.map((s, j) =>
                      j === i ? { ...s, rangeMin: v } : s,
                    ),
                  }))
                }
              />
            </div>
          </div>
        ))}
      </div>
    </section>
  )
}
