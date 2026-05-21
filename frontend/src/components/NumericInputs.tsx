import { useEffect, useState } from 'react'
import { cn } from '../lib/cn'

const INT_PARTIAL = /^-?\d*$/
const DECIMAL_PARTIAL = /^-?\d*\.?\d*$/

const inputBase =
  'w-full rounded border border-black/10 px-2 py-1 font-mono text-sm focus:border-leaf focus:outline-none focus:ring-2 focus:ring-leaf/20'

type BaseProps = {
  label?: string
  className?: string
  inputClassName?: string
  disabled?: boolean
}

/** Integer: allows "-" while typing; commits on blur. */
export function IntInput({
  label,
  value,
  onChange,
  className,
  inputClassName,
  disabled,
}: BaseProps & {
  value: number
  onChange: (v: number) => void
}) {
  const [text, setText] = useState(() => String(value))
  useEffect(() => {
    setText(String(value))
  }, [value])

  const flush = () => {
    const t = text.trim()
    if (t === '' || t === '-') {
      setText(String(value))
      return
    }
    const n = Number.parseInt(t, 10)
    if (!Number.isFinite(n)) {
      setText(String(value))
      return
    }
    onChange(n)
    setText(String(n))
  }

  return (
    <label className={cn('block text-xs', className)}>
      {label ? <span className="text-ink-faint">{label}</span> : null}
      <input
        type="text"
        inputMode="numeric"
        autoComplete="off"
        spellCheck={false}
        disabled={disabled}
        className={cn(label ? 'mt-0.5' : 'mt-0', inputBase, inputClassName)}
        value={text}
        onChange={(e) => {
          const raw = e.target.value
          if (INT_PARTIAL.test(raw)) {
            setText(raw)
            if (raw !== '' && raw !== '-') {
              const n = Number.parseInt(raw, 10)
              if (Number.isFinite(n)) onChange(n)
            }
          }
        }}
        onBlur={flush}
      />
    </label>
  )
}

/** Nullable integer (e.g. segment Range min): empty → null. */
export function NullableIntInput({
  label,
  value,
  onChange,
  className,
  inputClassName,
  disabled,
}: BaseProps & {
  value: number | null
  onChange: (v: number | null) => void
}) {
  const [text, setText] = useState(() => (value === null ? '' : String(value)))
  useEffect(() => {
    setText(value === null ? '' : String(value))
  }, [value])

  const flush = () => {
    const t = text.trim()
    if (t === '') {
      onChange(null)
      setText('')
      return
    }
    if (t === '-') {
      setText(value === null ? '' : String(value))
      return
    }
    const n = Number.parseInt(t, 10)
    if (!Number.isFinite(n)) {
      setText(value === null ? '' : String(value))
      return
    }
    onChange(n)
    setText(String(n))
  }

  return (
    <label className={cn('block text-xs', className)}>
      {label ? <span className="text-ink-faint">{label}</span> : null}
      <input
        type="text"
        inputMode="numeric"
        autoComplete="off"
        spellCheck={false}
        disabled={disabled}
        className={cn(label ? 'mt-0.5' : 'mt-0', inputBase, inputClassName)}
        value={text}
        onChange={(e) => {
          const raw = e.target.value
          if (INT_PARTIAL.test(raw)) {
            setText(raw)
            if (raw !== '' && raw !== '-') {
              const n = Number.parseInt(raw, 10)
              if (Number.isFinite(n)) onChange(n)
            }
          }
        }}
        onBlur={flush}
      />
    </label>
  )
}

/** Decimal input; optional fixed decimal places on blur/display. */
export function DecimalInput({
  label,
  value,
  onChange,
  className,
  inputClassName,
  disabled,
  decimalPlaces,
}: BaseProps & {
  value: number
  onChange: (v: number) => void
  /** When set, display and commit values rounded to this many decimal places. */
  decimalPlaces?: number
}) {
  const format = (n: number) =>
    decimalPlaces !== undefined ? formatFixedDecimal(n, decimalPlaces) : formatDecimal(n)

  const [text, setText] = useState(() => format(value))
  useEffect(() => {
    setText(format(value))
  }, [value, decimalPlaces])

  const flush = () => {
    const t = text.trim().replace(',', '.')
    if (t === '' || t === '-' || t === '.' || t === '-.') {
      setText(format(value))
      return
    }
    const n = Number.parseFloat(t)
    if (!Number.isFinite(n)) {
      setText(format(value))
      return
    }
    const committed =
      decimalPlaces !== undefined ? roundToDecimalPlaces(n, decimalPlaces) : n
    onChange(committed)
    setText(format(committed))
  }

  return (
    <label className={cn('block text-xs', className)}>
      {label ? <span className="text-ink-faint">{label}</span> : null}
      <input
        type="text"
        inputMode="decimal"
        autoComplete="off"
        spellCheck={false}
        disabled={disabled}
        className={cn(label ? 'mt-0.5' : 'mt-0', inputBase, inputClassName)}
        value={text}
        onChange={(e) => {
          const raw = e.target.value.replace(',', '.')
          if (DECIMAL_PARTIAL.test(raw)) {
            setText(raw)
            if (raw !== '' && raw !== '-' && raw !== '.' && raw !== '-.') {
              const n = Number.parseFloat(raw)
              if (Number.isFinite(n)) onChange(n)
            }
          }
        }}
        onBlur={flush}
      />
    </label>
  )
}

function formatDecimal(n: number): string {
  if (!Number.isFinite(n)) return '0'
  const s = String(n)
  if (s.includes('e') || s.includes('E')) return n.toFixed(6).replace(/\.?0+$/, '')
  return s
}

function formatFixedDecimal(n: number, places: number): string {
  if (!Number.isFinite(n)) return (0).toFixed(places)
  return roundToDecimalPlaces(n, places).toFixed(places)
}

function roundToDecimalPlaces(n: number, places: number): number {
  const p = 10 ** places
  return Math.round(n * p) / p
}

/**
 * Edits relevance as **percent** (e.g. 50 for 50%) while the parent keeps a **fraction** (0.5).
 */
export function FractionAsPercentInput({
  label,
  valueFraction,
  onChangeFraction,
  className,
  inputClassName,
  disabled,
  /** When false, parent is only notified on blur (manual relevance before Recalculate Points). */
  notifyParentWhileTyping = false,
  decimalPlaces = 2,
}: BaseProps & {
  valueFraction: number
  onChangeFraction: (fraction: number) => void
  notifyParentWhileTyping?: boolean
  decimalPlaces?: number
}) {
  const [text, setText] = useState(() => formatPercentText(valueFraction, decimalPlaces))
  useEffect(() => {
    setText(formatPercentText(valueFraction, decimalPlaces))
  }, [valueFraction, decimalPlaces])

  const flush = () => {
    const t = text.trim().replace(',', '.')
    if (t === '' || t === '-' || t === '.' || t === '-.') {
      setText(formatPercentText(valueFraction, decimalPlaces))
      return
    }
    const pct = Number.parseFloat(t)
    if (!Number.isFinite(pct)) {
      setText(formatPercentText(valueFraction, decimalPlaces))
      return
    }
    const fraction = roundToDecimalPlaces(pct / 100, decimalPlaces + 2)
    onChangeFraction(fraction)
    setText(formatPercentText(fraction, decimalPlaces))
  }

  return (
    <label className={cn('block text-xs', className)}>
      {label ? <span className="text-ink-faint">{label}</span> : null}
      <input
        type="text"
        inputMode="decimal"
        autoComplete="off"
        spellCheck={false}
        disabled={disabled}
        className={cn(label ? 'mt-0.5' : 'mt-0', inputBase, inputClassName)}
        value={text}
        onChange={(e) => {
          const raw = e.target.value.replace(',', '.')
          if (DECIMAL_PARTIAL.test(raw)) {
            setText(raw)
            if (
              notifyParentWhileTyping &&
              raw !== '' &&
              raw !== '-' &&
              raw !== '.' &&
              raw !== '-.'
            ) {
              const pct = Number.parseFloat(raw)
              if (Number.isFinite(pct)) onChangeFraction(pct / 100)
            }
          }
        }}
        onBlur={flush}
      />
    </label>
  )
}

function formatPercentText(fraction: number, decimalPlaces = 2): string {
  if (!Number.isFinite(fraction)) return (0).toFixed(decimalPlaces)
  return (fraction * 100).toFixed(decimalPlaces)
}
