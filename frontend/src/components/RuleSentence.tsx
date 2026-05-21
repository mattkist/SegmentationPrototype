import type { ReactNode } from 'react'
import { cn } from '../lib/cn'
import { DecimalInput, IntInput } from './NumericInputs'

const inlineInput = 'inline-block w-16 align-middle'

type InlineIntProps = {
  value: number
  onChange: (v: number) => void
  className?: string
}

export function InlineInt({ value, onChange, className }: InlineIntProps) {
  return (
    <IntInput
      value={value}
      onChange={onChange}
      className={cn('inline-block', className)}
      inputClassName={cn(inlineInput, 'text-center')}
    />
  )
}

export function InlineDecimal({ value, onChange, className }: InlineIntProps) {
  return (
    <DecimalInput
      value={value}
      onChange={onChange}
      className={cn('inline-block', className)}
      inputClassName={cn(inlineInput, 'text-center')}
    />
  )
}

/** Module (hectares) bounds: one decimal place (e.g. 2.5). */
export function InlineModuleHa({ value, onChange, className }: InlineIntProps) {
  return (
    <DecimalInput
      value={value}
      onChange={onChange}
      decimalPlaces={1}
      className={cn('inline-block', className)}
      inputClassName={cn(inlineInput, 'text-center')}
    />
  )
}

/** Trailing label after a score input in rule sentences. */
export function PointsLabel() {
  return <span className="font-medium text-ink-muted">points</span>
}

export function RuleSentence({
  children,
  className,
}: {
  children: ReactNode
  className?: string
}) {
  return (
    <p
      className={cn(
        'flex flex-wrap items-center gap-x-1.5 gap-y-2 text-sm leading-relaxed text-ink',
        className,
      )}
    >
      {children}
    </p>
  )
}

export function RuleBlock({
  children,
  onRemove,
  className,
}: {
  children: ReactNode
  onRemove?: () => void
  className?: string
}) {
  return (
    <div className={cn('space-y-2 rounded-lg border border-black/5 bg-surface-muted/30 p-4', className)}>
      {children}
      {onRemove && (
        <button
          type="button"
          onClick={onRemove}
          className="text-xs font-medium text-red-700 hover:underline"
        >
          Remove
        </button>
      )}
    </div>
  )
}

