import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { deriveKpiMaxScores, type KpiMaxKey } from '../../domain/deriveKpiMax'
import { getCultureTypeBlock, patchCultureTypeBlock } from '../../domain/cultureTypeDraft'
import { FieldLabel } from '../../components/Hint'
import { IntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'
import { cn } from '../../lib/cn'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

const kpiRows: { id: KpiMaxKey; label: string }[] = [
  { id: 'loyalty', label: 'Loyalty' },
  { id: 'quality', label: 'Quality' },
  { id: 'financial', label: 'Financial' },
  { id: 'technology', label: 'Technology' },
  { id: 'esg', label: 'ESG' },
  { id: 'yield', label: 'Yield' },
  { id: 'scale', label: 'Scale' },
]

function relevancePercent(configured: number, maximum: number): string {
  if (maximum <= 0) return '0.00'
  return ((configured / maximum) * 100).toFixed(2)
}

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

  const patchKpiMax = (key: KpiMaxKey, value: number) => {
    setDraft((prev) =>
      patchCultureTypeBlock(prev, cultureTypeCode, (b) => {
        switch (key) {
          case 'loyalty':
            return { ...b, loyalty: { ...b.loyalty, maxScore: value } }
          case 'quality':
            return { ...b, quality: { ...b.quality, maxScore: value } }
          case 'financial':
            return { ...b, financial: { ...b.financial, maxScore: value } }
          case 'technology':
            return { ...b, technology: { ...b.technology, maxScore: value } }
          case 'esg':
            return { ...b, esg: { ...b.esg, maxScore: value } }
          case 'yield':
            return { ...b, yield: { ...b.yield, maxScore: value } }
          case 'scale':
            return { ...b, scale: { ...b.scale, maxScore: value } }
        }
      }),
    )
  }

  const totalsOk = d.matchesMaximum && d.allKpiRulesMatchConfigured

  return (
    <section className="rounded-xl border border-black/5 bg-surface-card p-4 shadow-card">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <label className="block text-sm">
          <FieldLabel label="Maximum score (culture type)" hint={hints.configMaximumScore} />
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
        <p
          className={cn(
            'text-sm font-medium tabular-nums',
            d.matchesMaximum ? 'text-leaf' : 'text-amber-700',
          )}
        >
          Sum of KPI max scores: {d.sumConfigured} / {M}
        </p>
      </div>

      <div className="mt-4 overflow-x-auto">
        <table className="w-full min-w-[720px] text-left text-xs">
          <thead className="text-ink-faint">
            <tr>
              <th className="pb-2 pr-4">KPI</th>
              <th className="pb-2 pr-4">Configured max</th>
              <th className="pb-2 pr-4">Derived from rules</th>
              <th className="pb-2">Relevance %</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-black/5">
            {kpiRows.map((row) => {
              const configured = d.configured[row.id]
              const derived = d[row.id]
              const rulesOk = d.kpiMatches(row.id)
              return (
                <tr key={row.id}>
                  <td className="py-2 pr-4 font-medium text-ink">{row.label}</td>
                  <td className="py-2 pr-4">
                    <IntInput
                      value={configured}
                      onChange={(v) => patchKpiMax(row.id, v)}
                      className="w-24"
                    />
                  </td>
                  <td
                    className={cn(
                      'py-2 pr-4 font-mono tabular-nums',
                      rulesOk ? 'text-ink-muted' : 'text-amber-700',
                    )}
                  >
                    {derived}
                    {!rulesOk && (
                      <span className="ml-2 text-amber-700">(does not match rules)</span>
                    )}
                  </td>
                  <td className="py-2 font-mono tabular-nums text-ink-muted">
                    {relevancePercent(configured, M)}%
                  </td>
                </tr>
              )
            })}
          </tbody>
        </table>
      </div>

      {!totalsOk && (
        <p className="mt-3 text-sm text-amber-700">
          Save is disabled until the sum of configured KPI max scores equals the culture type maximum
          ({M}) and each KPI&apos;s configured max matches the derived cap from its rules.
        </p>
      )}
    </section>
  )
}
