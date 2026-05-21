import type { Dispatch, SetStateAction } from 'react'
import type {
  CultureTypeConfigurationWriteDto,
  SaveSegmentationConfigurationDto,
} from '../../api/types'
import { getCultureTypeBlock, patchCultureTypeBlock } from '../../domain/cultureTypeDraft'

export type SetDraft = Dispatch<SetStateAction<SaveSegmentationConfigurationDto>>

export function useCultureTypeEditor(
  draft: SaveSegmentationConfigurationDto,
  setDraft: SetDraft,
  cultureTypeCode: string,
) {
  const block = getCultureTypeBlock(draft, cultureTypeCode)
  const patchBlock = (
    fn: (b: CultureTypeConfigurationWriteDto) => CultureTypeConfigurationWriteDto,
  ) => setDraft((d) => patchCultureTypeBlock(d, cultureTypeCode, fn))

  return { block, patchBlock }
}
