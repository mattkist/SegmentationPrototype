import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { deriveKpiMaxScores } from '../../domain/deriveKpiMax'
import { recalculatePointsFromRelevances, setKpiRelevance, type KpiKey } from '../../domain/kpiRelevanceCaps'
import { getCultureTypeBlock, patchCultureTypeBlock } from '../../domain/cultureTypeDraft'
import { isRelevanceSumExactly100, relevanceSumPercent } from '../../domain/relevanceDisplay'
import { FieldLabel } from '../../components/Hint'
import { FractionAsPercentInput, IntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'
import { cn } from '../../lib/cn'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function KpiTotalsBanner({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  const block = getCultureTypeBlock(draft, cultureTypeCode)
  const d = deriveKpiMaxScores(block)
  const M = block.maximumScore
  const relevanceTotal = relevanceSumPercent(block)
  const canRecalculate = isRelevanceSumExactly100(block) && M > 0

  const rows: {
    id: KpiKey
    label: string
    cap: number
    relevance: number
  }[] = [
    { id: 'loyalty', label: 'Loyalty', cap: d.loyalty, relevance: block.loyalty.relevance },
    { id: 'quality', label: 'Quality', cap: d.quality, relevance: block.quality.relevance },
    { id: 'financial', label: 'Financial', cap: d.financial, relevance: block.financial.relevance },
    { id: 'technology', label: 'Technology', cap: d.technology, relevance: block.technology.relevance },
    { id: 'esg', label: 'ESG (cap part)', cap: d.esg, relevance: block.esg.relevance },
    { id: 'yield', label: 'Yield', cap: d.yield, relevance: block.yield.relevance },
    { id: 'scale', label: 'Scale', cap: d.scale, relevance: block.scale.relevance },
    {
      id: 'yieldAndScale',
      label: 'Yield & Scale',
      cap: d.yieldAndScale,
      relevance: block.yieldAndScale.relevance,
    },
  ]

  return (
    <section className="rounded-xl border border-black/5 bg-surface-card p-4 shadow-card">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <label className="block text-sm">
          <FieldLabel label="Maximum score" hint={hints.configMaximumScore} />
          <IntInput
            value={block.maximumScore}
            onChange={(v) =>
              setDraft((prev) =>
                patchCultureTypeBlock(prev, cultureTypeCode, (b) => ({ ...b, maximumScore: v })),
              )
            }
            className="mt-1 w-32"
          />
        </label>
        <div className="flex flex-wrap items-end gap-3">
          <p
            className={cn(
              'text-sm font-medium tabular-nums',
              d.matchesMaximum ? 'text-leaf' : 'text-amber-700',
            )}
          >
            KPI caps sum: {d.sum} / {M}
          </p>
          <button
            type="button"
            disabled={!canRecalculate}
            title={
              canRecalculate
                ? 'Rescale all KPI rule scores to match relevance shares'
                : 'Relevance % must sum to exactly 100.00'
            }
            onClick={() =>
              setDraft((prev) =>
                patchCultureTypeBlock(prev, cultureTypeCode, recalculatePointsFromRelevances),
              )
            }
            className={cn(
              'rounded-lg px-3 py-1.5 text-xs font-semibold text-white shadow-sm',
              canRecalculate
                ? 'bg-leaf hover:bg-leaf-hover'
                : 'cursor-not-allowed bg-ink-faint',
            )}
          >
            Recalculate Points
          </button>
        </div>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="w-full min-w-[640px] text-left text-xs">
          <thead className="text-ink-faint">
            <tr>
              <th className="pb-2 pr-4">KPI</th>
              <th className="pb-2 pr-4">Derived cap</th>
              <th className="pb-2">Relevance %</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-black/5">
            {rows.map((row) => (
              <tr key={row.id}>
                <td className="py-2 pr-4 font-medium text-ink">{row.label}</td>
                <td className="py-2 pr-4 font-mono tabular-nums">{row.cap}</td>
                <td className="py-2">
                  <FractionAsPercentInput
                    valueFraction={row.relevance}
                    onChangeFraction={(frac: number) =>
                      setDraft((prev) =>
                        patchCultureTypeBlock(prev, cultureTypeCode, (b) =>
                          setKpiRelevance(b, row.id, frac),
                        ),
                      )
                    }
                    notifyParentWhileTyping={false}
                  />
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
      <p className="mt-2 text-xs text-ink-muted">
        Relevance sum:{' '}
        <span className={cn('font-mono tabular-nums', canRecalculate ? 'text-leaf' : 'text-amber-700')}>
          {relevanceTotal.toFixed(2)}%
        </span>{' '}
        — must be 100.00% before using Recalculate Points. Percent inputs show 2 decimal places.
      </p>
    </section>
  )
}
