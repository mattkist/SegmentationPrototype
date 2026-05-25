import type { SegmentShareDto } from '../../api/types'
import { cn } from '../../lib/cn'

const BAR_COLORS = [
  'bg-leaf',
  'bg-accent',
  'bg-amber-500',
  'bg-sky-500',
  'bg-violet-500',
  'bg-rose-400',
]

export function SegmentDistributionChart({
  title,
  segments,
  className,
}: {
  title: string
  segments: SegmentShareDto[]
  className?: string
}) {
  if (segments.length === 0) {
    return (
      <div className={cn('rounded-xl border border-black/5 bg-surface-card p-4', className)}>
        <h3 className="text-sm font-semibold text-ink">{title}</h3>
        <p className="mt-2 text-sm text-ink-muted">No scored farmers in this group.</p>
      </div>
    )
  }

  return (
    <div className={cn('rounded-xl border border-black/5 bg-surface-card p-4 shadow-card', className)}>
      <h3 className="text-sm font-semibold text-ink">{title}</h3>
      <ul className="mt-4 space-y-3">
        {segments.map((s, i) => (
          <li key={s.segmentName}>
            <div className="flex items-center justify-between gap-2 text-xs">
              <span className="font-medium text-ink">{s.segmentName}</span>
              <span className="font-mono tabular-nums text-ink-muted">
                {s.farmerCount} ({s.percentage.toFixed(1)}%)
              </span>
            </div>
            <div className="mt-1 h-2 overflow-hidden rounded-full bg-surface-muted">
              <div
                className={cn('h-full rounded-full transition-all', BAR_COLORS[i % BAR_COLORS.length])}
                style={{ width: `${Math.min(100, Math.max(0, s.percentage))}%` }}
              />
            </div>
          </li>
        ))}
      </ul>
    </div>
  )
}
