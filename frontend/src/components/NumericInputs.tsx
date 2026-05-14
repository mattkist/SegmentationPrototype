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

/** Decimal (relevance, etc.): allows "-" and "." while typing. */
export function DecimalInput({
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
  const [text, setText] = useState(() => formatDecimal(value))
  useEffect(() => {
    setText(formatDecimal(value))
  }, [value])

  const flush = () => {
    const t = text.trim().replace(',', '.')
    if (t === '' || t === '-' || t === '.' || t === '-.') {
      setText(formatDecimal(value))
      return
    }
    const n = Number.parseFloat(t)
    if (!Number.isFinite(n)) {
      setText(formatDecimal(value))
      return
    }
    onChange(n)
    setText(formatDecimal(n))
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
  /** When false, parent is only notified on blur (avoids heavy rescale on every keystroke). */
  notifyParentWhileTyping = true,
}: BaseProps & {
  valueFraction: number
  onChangeFraction: (fraction: number) => void
  notifyParentWhileTyping?: boolean
}) {
  const [text, setText] = useState(() => formatPercentText(valueFraction))
  useEffect(() => {
    setText(formatPercentText(valueFraction))
  }, [valueFraction])

  const flush = () => {
    const t = text.trim().replace(',', '.')
    if (t === '' || t === '-' || t === '.' || t === '-.') {
      setText(formatPercentText(valueFraction))
      return
    }
    const pct = Number.parseFloat(t)
    if (!Number.isFinite(pct)) {
      setText(formatPercentText(valueFraction))
      return
    }
    onChangeFraction(pct / 100)
    setText(formatPercentText(pct / 100))
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

function formatPercentText(fraction: number): string {
  if (!Number.isFinite(fraction)) return '0'
  const pct = fraction * 100
  return formatDecimal(pct)
}
