import * as Tooltip from '@radix-ui/react-tooltip'
import type { ReactNode } from 'react'
import { CircleHelp } from 'lucide-react'
import { cn } from '../lib/cn'

export function HintProvider({ children }: { children: ReactNode }) {
  return (
    <Tooltip.Provider delayDuration={280} skipDelayDuration={200}>
      {children}
    </Tooltip.Provider>
  )
}

type HintProps = {
  content: string
  side?: 'top' | 'right' | 'bottom' | 'left'
  className?: string
  children?: ReactNode
}

export function Hint({ content, side = 'top', className, children }: HintProps) {
  return (
    <Tooltip.Root>
      <Tooltip.Trigger asChild>
        {children ?? (
          <button
            type="button"
            className={cn(
              'inline-flex shrink-0 rounded-full p-0.5 text-ink-faint transition hover:bg-surface-muted hover:text-leaf',
              className,
            )}
            aria-label="Explain this field"
          >
            <CircleHelp className="h-4 w-4" aria-hidden />
          </button>
        )}
      </Tooltip.Trigger>
      <Tooltip.Portal>
        <Tooltip.Content
          side={side}
          sideOffset={6}
          className="z-50 max-w-sm rounded-lg border border-zinc-700 bg-zinc-900 px-3 py-2 text-left text-xs leading-relaxed text-zinc-100 shadow-xl"
        >
          {content}
          <Tooltip.Arrow className="fill-zinc-900" />
        </Tooltip.Content>
      </Tooltip.Portal>
    </Tooltip.Root>
  )
}

export function FieldLabel({
  label,
  hint,
  className,
}: {
  label: string
  hint: string
  className?: string
}) {
  return (
    <div className={cn('flex items-center gap-1.5 text-sm font-medium text-ink', className)}>
      <span>{label}</span>
      <Hint content={hint} />
    </div>
  )
}

export function SectionTitle({
  title,
  hint,
  className,
}: {
  title: string
  hint: string
  className?: string
}) {
  return (
    <div className={cn('mb-3 flex items-center gap-2', className)}>
      <h3 className="font-display text-lg font-semibold text-ink">{title}</h3>
      <Hint content={hint} />
    </div>
  )
}
