/**
 * API / DB store KPI relevance as a **fraction** (e.g. 0.15 = 15%).
 * Values in (1, 100] are treated as "percent stored as whole number" (15 → 0.15).
 * Accepts `unknown` so JSON numbers serialized as strings still parse.
 */
export function relevanceFieldToFraction(raw: unknown): number {
  if (raw === undefined || raw === null) return 0
  const n = typeof raw === 'number' ? raw : Number(raw)
  if (!Number.isFinite(n)) return 0
  if (n > 1.000001 && n <= 100.000001) return n / 100
  return n
}

/** @deprecated use {@link relevanceFieldToFraction} */
export const relevanceFractionFromApi = relevanceFieldToFraction
