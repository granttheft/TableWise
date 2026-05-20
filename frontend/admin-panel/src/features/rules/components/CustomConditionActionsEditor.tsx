import { useMemo } from 'react'
import { Label } from '@/components/ui/label'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { Switch } from '@/components/ui/switch'
import { Slider } from '@/components/ui/slider'
import { Button } from '@/components/ui/button'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { Code } from 'lucide-react'
import type { CustomActionsFormState } from '../utils/customActionsForm'
import {
  buildCustomActionsJsonFromForm,
  parseCustomActionsJson,
} from '../utils/customActionsForm'
import { validateCustomActionsJson } from '../utils/validateCustomActionsJson'
import { cn } from '@/lib/cn'

interface CustomConditionActionsEditorProps {
  jsonValue: string
  onJsonChange: (json: string) => void
  form: CustomActionsFormState
  onFormChange: (form: CustomActionsFormState) => void
  rawJsonMode: boolean
  onRawJsonModeChange: (raw: boolean) => void
}

const ACTION_FIELD_HELP: { key: keyof CustomActionsFormState; label: string; hint: string }[] = [
  { key: 'block', label: 'Engelle (block)', hint: 'Rezervasyonu tamamen engeller' },
  { key: 'warn', label: 'Uyar (warn)', hint: 'Uyarı gösterir, izin verebilir' },
  { key: 'suggest', label: 'Öner (suggest)', hint: 'Alternatif öneri sunar' },
  { key: 'requireDeposit', label: 'Kapora (requireDeposit)', hint: 'Kapora ödemesi ister' },
]

/**
 * Özel kural aksiyonları: form modu veya ham JSON; arka planda actionsJson üretilir.
 */
export function CustomConditionActionsEditor({
  jsonValue,
  onJsonChange,
  form,
  onFormChange,
  rawJsonMode,
  onRawJsonModeChange,
}: CustomConditionActionsEditorProps) {
  const validation = useMemo(
    () => validateCustomActionsJson(rawJsonMode ? jsonValue : buildCustomActionsJsonFromForm(form)),
    [jsonValue, form, rawJsonMode]
  )

  const syncFormToJson = (next: CustomActionsFormState) => {
    onFormChange(next)
    if (!rawJsonMode) {
      onJsonChange(buildCustomActionsJsonFromForm(next))
    }
  }

  const handleRawChange = (text: string) => {
    onJsonChange(text)
    if (rawJsonMode) {
      onFormChange(parseCustomActionsJson(text))
    }
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between gap-2">
        <div>
          <h4 className="font-medium text-sm">Aksiyonlar (custom_condition)</h4>
          <p className="text-xs text-muted-foreground">
            Form ile düzenleyin veya ham JSON moduna geçin
          </p>
        </div>
        <Button
          type="button"
          variant="outline"
          size="sm"
          onClick={() => {
            const next = !rawJsonMode
            onRawJsonModeChange(next)
            if (next) {
              onJsonChange(buildCustomActionsJsonFromForm(form))
            } else {
              onFormChange(parseCustomActionsJson(jsonValue))
            }
          }}
        >
          <Code className="h-4 w-4 mr-1" />
          {rawJsonMode ? 'Form modu' : 'Ham JSON'}
        </Button>
      </div>

      {rawJsonMode ? (
        <div className="space-y-2">
          <Label>Aksiyon JSON</Label>
          <Textarea
            value={jsonValue}
            onChange={(e) => handleRawChange(e.target.value)}
            rows={10}
            className={cn(
              'font-mono text-sm',
              !validation.valid && 'border-destructive ring-1 ring-destructive/30'
            )}
          />
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {ACTION_FIELD_HELP.map(({ key, label, hint }) => (
            <div
              key={key}
              className="flex items-center justify-between rounded-lg border p-3"
            >
              <div className="space-y-0.5 pr-2">
                <Label className="text-sm">{label}</Label>
                <p className="text-xs text-muted-foreground">{hint}</p>
              </div>
              <Switch
                checked={Boolean(form[key])}
                onCheckedChange={(checked) =>
                  syncFormToJson({ ...form, [key]: checked })
                }
              />
            </div>
          ))}

          <div className="sm:col-span-2 space-y-2">
            <Label>Mesaj (message)</Label>
            <Input
              value={form.message}
              onChange={(e) => syncFormToJson({ ...form, message: e.target.value })}
              placeholder="Misafire gösterilecek metin"
            />
          </div>

          <div className="space-y-2">
            <div className="flex justify-between">
              <Label>İndirim % (discountPercent)</Label>
              <span className="text-xs text-muted-foreground">
                {form.discountPercent != null ? `%${form.discountPercent}` : 'Kapalı'}
              </span>
            </div>
            <Slider
              value={[form.discountPercent ?? 0]}
              onValueChange={([v]) =>
                syncFormToJson({
                  ...form,
                  discountPercent: v <= 0 ? null : Math.min(50, v),
                })
              }
              min={0}
              max={50}
              step={1}
            />
          </div>

          {form.requireDeposit ? (
            <div className="space-y-2">
              <Label>Kapora tutarı (depositAmount, ₺)</Label>
              <Input
                type="number"
                min={0}
                value={form.depositAmount ?? 100}
                onChange={(e) => {
                  const n = parseInt(e.target.value, 10)
                  syncFormToJson({
                    ...form,
                    depositAmount: Number.isNaN(n) ? 100 : n,
                  })
                }}
              />
            </div>
          ) : null}
        </div>
      )}

      {!validation.valid ? (
        <Alert variant="destructive" className="py-2">
          <AlertDescription>
            <ul className="list-disc pl-4 space-y-1 text-xs">
              {validation.issues.map((issue, i) => (
                <li key={`${issue.path}-${i}`}>
                  <span className="font-mono">{issue.path}</span>: {issue.message}
                </li>
              ))}
            </ul>
          </AlertDescription>
        </Alert>
      ) : (
        <p className="text-xs text-green-600 dark:text-green-400">Aksiyon JSON geçerli</p>
      )}
    </div>
  )
}
