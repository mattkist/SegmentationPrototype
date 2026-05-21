import type {
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
} from '../api/types'
import { createCultureTypeBlock } from './defaultConfiguration'

export type CultureTypeCatalogItem = { code: string; name: string }

/** Normalizes API rows (camelCase or PascalCase) into catalog items. */
export function normalizeCultureTypesCatalog(raw: unknown): CultureTypeCatalogItem[] {
  if (!Array.isArray(raw)) return []
  return raw
    .map((item): CultureTypeCatalogItem | null => {
      if (!item || typeof item !== 'object') return null
      const o = item as Record<string, unknown>
      const code = String(o.code ?? o.Code ?? '').trim()
      const name = String(o.name ?? o.Name ?? code).trim()
      if (!code) return null
      return { code, name }
    })
    .filter((x): x is CultureTypeCatalogItem => x !== null)
}

export function draftHasAllCatalogCultureTypes(
  draft: SaveSegmentationConfigurationDto,
  catalog: CultureTypeCatalogItem[],
): boolean {
  if (catalog.length === 0) return true
  const expected = catalog.map((c) => c.code).sort().join('\0')
  const actual = draft.cultureTypes.map((c) => c.cultureTypeCode).sort().join('\0')
  return expected === actual
}

export function getCultureTypeBlock(
  draft: SaveSegmentationConfigurationDto,
  cultureTypeCode: string,
): CultureTypeConfigurationWriteDto {
  const block = draft.cultureTypes.find((c) => c.cultureTypeCode === cultureTypeCode)
  if (!block) throw new Error(`Culture type '${cultureTypeCode}' not found in draft`)
  return block
}

export function patchCultureTypeBlock(
  draft: SaveSegmentationConfigurationDto,
  cultureTypeCode: string,
  patch: (block: CultureTypeConfigurationWriteDto) => CultureTypeConfigurationWriteDto,
): SaveSegmentationConfigurationDto {
  return {
    ...draft,
    cultureTypes: draft.cultureTypes.map((ct) =>
      ct.cultureTypeCode === cultureTypeCode ? patch(ct) : ct,
    ),
  }
}

/** Ensures one culture-type block per catalog code (order follows catalog). */
export function mergeCultureTypesFromCatalog(
  draft: SaveSegmentationConfigurationDto,
  catalog: CultureTypeCatalogItem[],
): SaveSegmentationConfigurationDto {
  if (catalog.length === 0) return draft
  const byCode = new Map(draft.cultureTypes.map((ct) => [ct.cultureTypeCode, ct]))
  const cultureTypes = catalog.map((c) => byCode.get(c.code) ?? createCultureTypeBlock(c.code))
  return syncSegmentThresholdsForHeader({ ...draft, cultureTypes })
}

export function syncSegmentThresholdsForHeader(
  draft: SaveSegmentationConfigurationDto,
): SaveSegmentationConfigurationDto {
  return {
    ...draft,
    cultureTypes: draft.cultureTypes.map((ct) => ({
      ...ct,
      segmentThresholds: draft.segments.map((s) => {
        const existing = ct.segmentThresholds.find(
          (t) => t.segmentName.toLowerCase() === s.segmentName.toLowerCase(),
        )
        return {
          segmentId: s.id ?? existing?.segmentId,
          segmentName: s.segmentName,
          rangeMin: existing?.rangeMin ?? null,
        }
      }),
    })),
  }
}
