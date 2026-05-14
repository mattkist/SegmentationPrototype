import type { Dispatch, SetStateAction } from 'react'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { deriveKpiMaxScores } from '../../domain/deriveKpiMax'
import { applyRelevanceFractionEdit, type KpiKey } from '../../domain/kpiRelevanceCaps'
import { Hint } from '../../components/Hint'
import { FractionAsPercentInput, IntInput } from '../../components/NumericInputs'
import { hints } from '../../hints/en'
import { cn } from '../../lib/cn'

type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function KpiTotalsBanner({
  draft,
  setDraft,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
}) {
  const d = deriveKpiMaxScores(draft)
  const M = draft.maximumScore

  const rows: {
    id: KpiKey
    label: string
    cap: number
    relevance: number
  }[] = [
    { id: 'loyalty', label: 'Loyalty', cap: d.loyalty, relevance: draft.loyalty.relevance },
    { id: 'quality', label: 'Quality', cap: d.quality, relevance: draft.quality.relevance },
    { id: 'financial', label: 'Financial', cap: d.financial, relevance: draft.financial.relevance },
    { id: 'technology', label: 'Technology', cap: d.technology, relevance: draft.technology.relevance },
    { id: 'esg', label: 'ESG (cap part)', cap: d.esg, relevance: draft.esg.relevance },
    { id: 'yield', label: 'Yield', cap: d.yield, relevance: draft.yield.relevance },
    { id: 'scale', label: 'Scale', cap: d.scale, relevance: draft.scale.relevance },
  ]

  const sumRelevancePct =
    (draft.loyalty.relevance +
      draft.quality.relevance +
      draft.financial.relevance +
      draft.technology.relevance +
      draft.esg.relevance +
      draft.yield.relevance +
      draft.scale.relevance) *
    100

  return (
    <div
      className={cn(
        'rounded-xl border p-4 shadow-sm',
        d.matchesMaximum
          ? 'border-leaf/40 bg-leaf-soft/50'
          : 'border-accent/40 bg-accent-soft/40',
      )}
    >
      <div className="mb-3 flex flex-wrap items-center gap-2">
        <span className="text-sm font-semibold text-ink">Derived KPI caps vs maximum</span>
        <Hint content={hints.configMaximumScore} />
        <Hint content={hints.configRelevance} />
      </div>

      {!d.matchesMaximum && (
        <p className="mb-2 rounded-lg border border-amber-200/80 bg-amber-50/90 px-3 py-2 text-xs text-amber-950">
          A soma dos derived max ({d.sum}) precisa igualar o <strong>maximum score</strong> ({M}) para editar pesos por
          relevância. Ajuste as pontuações nas abas ou o alvo abaixo.
        </p>
      )}

      {d.matchesMaximum && (
        <p className="mb-2 text-xs text-ink-muted">
          Relevância = cap do KPI ÷ maximum score (soma = 100%). Ao alterar um %, as <strong>pontuações internas</strong>{' '}
          desse e dos outros blocos são reescaladas para manter a soma dos caps = {M}.
        </p>
      )}

      <div className="overflow-x-auto rounded-lg border border-black/5 bg-surface-card">
        <table className="w-full min-w-[480px] border-collapse text-sm">
          <thead>
            <tr className="border-b border-black/10 bg-surface-muted/60 text-left text-xs font-semibold uppercase tracking-wide text-ink-muted">
              <th className="px-3 py-2">KPI block</th>
              <th className="px-3 py-2 text-right">Derived max</th>
              <th className="px-3 py-2 min-w-[7rem]">
                Relevance
                <span className="block font-normal normal-case text-ink-faint">(% do máx. total)</span>
              </th>
            </tr>
          </thead>
          <tbody>
            {rows.map((r) => (
              <tr key={r.id} className="border-b border-black/5 odd:bg-white/40">
                <td className="px-3 py-2 font-medium text-ink">{r.label}</td>
                <td className="px-3 py-2 text-right font-mono tabular-nums text-ink">{r.cap}</td>
                <td className="px-3 py-2 align-middle">
                  <div className="flex items-end gap-1">
                    <FractionAsPercentInput
                      valueFraction={r.relevance}
                      onChangeFraction={(fraction) =>
                        setDraft((prev) => applyRelevanceFractionEdit(prev, r.id, fraction))
                      }
                      notifyParentWhileTyping={false}
                      disabled={!d.matchesMaximum}
                      className="min-w-0 flex-1"
                      inputClassName="text-right"
                    />
                    <span className="shrink-0 pb-1 text-xs text-ink-muted">%</span>
                  </div>
                </td>
              </tr>
            ))}
            <tr className="border-t border-black/10 bg-surface-muted/30 text-sm">
              <td className="px-3 py-2 text-ink-muted">Σ relevance</td>
              <td className="px-3 py-2 text-right text-ink-muted">—</td>
              <td className="px-3 py-2 text-right font-mono tabular-nums font-semibold text-ink">
                {formatPct(sumRelevancePct)}%
              </td>
            </tr>
            <tr className="bg-surface-muted/40 font-semibold">
              <td className="px-3 py-2 text-ink">Sum of derived caps</td>
              <td className="px-3 py-2 text-right font-mono tabular-nums text-ink">{d.sum}</td>
              <td className="px-3 py-2 text-ink-muted text-xs">—</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div className="mt-3 flex flex-wrap items-center gap-3 text-sm">
        <span className="text-ink-muted">Target maximum score</span>
        <IntInput
          value={draft.maximumScore}
          onChange={(v) => setDraft((x) => ({ ...x, maximumScore: v }))}
          className="w-28 shrink-0"
        />
        {d.matchesMaximum ? (
          <span className="font-medium text-leaf">Matches sum — save allowed.</span>
        ) : (
          <span className="font-medium text-accent">
            Mismatch — adjust rule scores or target until sum equals target.
          </span>
        )}
      </div>
    </div>
  )
}

function formatPct(p: number): string {
  if (!Number.isFinite(p)) return '0'
  const rounded = Math.round(p * 100) / 100
  if (Math.abs(rounded - Math.round(rounded)) < 1e-9) return String(Math.round(rounded))
  return String(rounded)
}
