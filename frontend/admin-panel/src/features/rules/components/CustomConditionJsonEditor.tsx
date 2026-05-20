import { useMemo } from 'react'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Alert, AlertDescription } from '@/components/ui/alert'
import { CustomConditionFieldReferencePanel } from './CustomConditionFieldReferencePanel'
import { CUSTOM_CONDITION_EXAMPLES } from '../constants/customConditionExamples'
import { validateCustomConditionJson } from '../utils/validateCustomConditionJson'
import { cn } from '@/lib/cn'

interface CustomConditionJsonEditorProps {
  value: string
  onChange: (value: string) => void
  onLoadExample?: (conditionsJson: string, actionsJson: string) => void
  className?: string
}

/**
 * Özel kural koşul JSON editörü + field referans paneli + örnek yükleme.
 */
export function CustomConditionJsonEditor({
  value,
  onChange,
  onLoadExample,
  className,
}: CustomConditionJsonEditorProps) {
  const validation = useMemo(() => validateCustomConditionJson(value), [value])

  return (
    <div className={cn('space-y-3', className)}>
      <div className="flex flex-wrap items-center justify-between gap-2">
        <Label>Koşul JSON (custom_condition)</Label>
        <Select
          onValueChange={(id) => {
            const ex = CUSTOM_CONDITION_EXAMPLES.find((e) => e.id === id)
            if (!ex) return
            onChange(ex.conditionsJson)
            onLoadExample?.(ex.conditionsJson, ex.actionsJson)
          }}
        >
          <SelectTrigger className="w-[200px] h-8 text-xs">
            <SelectValue placeholder="Örnek yükle" />
          </SelectTrigger>
          <SelectContent>
            {CUSTOM_CONDITION_EXAMPLES.map((ex) => (
              <SelectItem key={ex.id} value={ex.id}>
                {ex.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <div className="grid gap-4 lg:grid-cols-[1fr_minmax(280px,360px)]">
        <div className="space-y-2 min-w-0">
          <Textarea
            value={value}
            onChange={(e) => onChange(e.target.value)}
            rows={16}
            spellCheck={false}
            className={cn(
              'font-mono text-sm leading-relaxed',
              !validation.valid && 'border-destructive ring-1 ring-destructive/30'
            )}
            aria-invalid={!validation.valid}
          />
          {!validation.valid ? (
            <Alert variant="destructive" className="py-2">
              <AlertDescription>
                <ul className="list-disc pl-4 space-y-1 text-xs">
                  {validation.issues.map((issue, i) => (
                    <li key={`${issue.path}-${i}`}>
                      <span className="font-mono">{issue.path}</span>
                      {issue.line != null ? ` (satır ${issue.line})` : ''}: {issue.message}
                    </li>
                  ))}
                </ul>
              </AlertDescription>
            </Alert>
          ) : (
            <p className="text-xs text-green-600 dark:text-green-400">JSON geçerli</p>
          )}
        </div>
        <CustomConditionFieldReferencePanel />
      </div>
    </div>
  )
}
