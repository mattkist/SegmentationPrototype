import type { Dispatch, SetStateAction } from 'react'
import type { FinancialSelfFundingRangeDto, SaveSegmentationConfigurationDto } from '../../api/types'
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

export function FinancialEditor({ draft, setDraft }: { draft: SaveSegmentationConfigurationDto; setDraft: SetDraft }) {
  const f = draft.financial
  const patchRow = (i: number, part: Partial<FinancialSelfFundingRangeDto>) => {
    setDraft((d) => ({
      ...d,
      financial: {
        ...d.financial,
        selfFundingRanges: d.financial.selfFundingRanges.map((x, j) => (j === i ? { ...x, ...part } : x)),
      },
    }))
  }

  return (
    <div className="space-y-6">
      <SectionTitle title="Financial" hint={hints.financialSf} />

      <div className="rounded-xl border border-black/5 bg-surface-card/50 p-4">
        <p className="mb-3 text-xs font-medium uppercase tracking-wide text-ink-muted">Debt rule</p>
        <div className="grid max-w-2xl gap-3 sm:grid-cols-2">
          <IntInput
            label="Debt crop season"
            value={f.debtCropSeason}
            onChange={(v) => setDraft((d) => ({ ...d, financial: { ...d.financial, debtCropSeason: v } }))}
          />
          <IntInput
            label="Debt score"
            value={f.debtScore}
            onChange={(v) => setDraft((d) => ({ ...d, financial: { ...d.financial, debtScore: v } }))}
          />
        </div>
      </div>

      <div>
        <div className="mb-2 flex justify-between">
          <FieldLabel label="Self-funding ranges" hint={hints.financialSf} />
          <button
            type="button"
            className="text-xs font-semibold text-leaf hover:underline"
            onClick={() =>
              setDraft((d) => ({
                ...d,
                financial: {
                  ...d.financial,
                  selfFundingRanges: [
                    ...d.financial.selfFundingRanges,
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
          {f.selfFundingRanges.map((r, i) => (
            <div key={i} className="grid gap-2 rounded-lg border border-black/5 p-3 sm:grid-cols-3 lg:grid-cols-6">
              <IntInput label="Min %" value={r.minimum} onChange={(v) => patchRow(i, { minimum: v })} />
              <IntInput label="Max %" value={r.maximum} onChange={(v) => patchRow(i, { maximum: v })} />
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
                <FieldLabel label="Skipped season ids" hint="Comma-separated ids." />
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
                    financial: {
                      ...d.financial,
                      selfFundingRanges: d.financial.selfFundingRanges.filter((_, j) => j !== i),
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
    </div>
  )
}
