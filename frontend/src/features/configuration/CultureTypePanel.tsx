import * as Tabs from '@radix-ui/react-tabs'
import type { SaveSegmentationConfigurationDto } from '../../api/types'
import { CultureTypeSettingsEditor } from './CultureTypeSettingsEditor'
import { KpiTotalsBanner } from './KpiTotalsBanner'
import { LoyaltyEditor } from './LoyaltyEditor'
import { QualityEditor } from './QualityEditor'
import { FinancialEditor } from './FinancialEditor'
import { EsgEditor, TechnologyEditor } from './TechnologyEsgEditors'
import { ScaleEditor, YieldEditor } from './YieldScaleEditors'
import type { SetDraft } from './cultureTypeEditorUtils'
import { cn } from '../../lib/cn'

const kpiTabs = [
  ['loyalty', 'Loyalty'],
  ['quality', 'Quality'],
  ['financial', 'Financial'],
  ['technology', 'Technology'],
  ['esg', 'ESG'],
  ['yield', 'Yield'],
  ['scale', 'Scale'],
] as const

export function CultureTypePanel({
  draft,
  setDraft,
  cultureTypeCode,
}: {
  draft: SaveSegmentationConfigurationDto
  setDraft: SetDraft
  cultureTypeCode: string
}) {
  return (
    <div className="space-y-6">
      <CultureTypeSettingsEditor
        draft={draft}
        setDraft={setDraft}
        cultureTypeCode={cultureTypeCode}
      />
      <KpiTotalsBanner draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />

      <Tabs.Root defaultValue="loyalty" className="space-y-4">
        <Tabs.List className="flex flex-wrap gap-1 rounded-xl border border-black/5 bg-surface-card p-1">
          {kpiTabs.map(([value, label]) => (
            <Tabs.Trigger
              key={value}
              value={value}
              className={cn(
                'rounded-lg px-3 py-2 text-sm font-medium',
                'data-[state=active]:bg-leaf data-[state=active]:text-white',
                'data-[state=inactive]:text-ink-muted data-[state=inactive]:hover:bg-surface-muted',
              )}
            >
              {label}
            </Tabs.Trigger>
          ))}
        </Tabs.List>
        {kpiTabs.map(([value]) => (
          <Tabs.Content
            key={value}
            value={value}
            className="rounded-xl border border-black/5 bg-surface-card p-6 shadow-card outline-none"
          >
            {value === 'loyalty' && (
              <LoyaltyEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'quality' && (
              <QualityEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'financial' && (
              <FinancialEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'technology' && (
              <TechnologyEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'esg' && (
              <EsgEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'yield' && (
              <YieldEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
            {value === 'scale' && (
              <ScaleEditor draft={draft} setDraft={setDraft} cultureTypeCode={cultureTypeCode} />
            )}
          </Tabs.Content>
        ))}
      </Tabs.Root>
    </div>
  )
}
