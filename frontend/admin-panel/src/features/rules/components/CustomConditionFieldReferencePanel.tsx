import { useState } from 'react'
import { ChevronDown, ChevronRight, Copy } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { toast } from 'sonner'
import { CUSTOM_CONDITION_FIELD_REFERENCE } from '../constants/customConditionFieldReference'
import { cn } from '@/lib/cn'

interface CustomConditionFieldReferencePanelProps {
  /** Varsayılan: açık */
  defaultOpen?: boolean
  className?: string
}

/**
 * Özel kural koşul JSON editörü yanında whitelist alan referans tablosu.
 */
export function CustomConditionFieldReferencePanel({
  defaultOpen = true,
  className,
}: CustomConditionFieldReferencePanelProps) {
  const [open, setOpen] = useState(defaultOpen)

  const copyField = async (field: string) => {
    try {
      await navigator.clipboard.writeText(field)
      toast.success('Alan adı kopyalandı')
    } catch {
      toast.error('Kopyalanamadı')
    }
  }

  return (
    <div className={cn('rounded-lg border bg-muted/30', className)}>
      <button
        type="button"
        className="flex w-full items-center justify-between px-3 py-2 text-left text-sm font-medium hover:bg-muted/50"
        onClick={() => setOpen((v) => !v)}
      >
        <span>Field Referansı</span>
        {open ? <ChevronDown className="h-4 w-4" /> : <ChevronRight className="h-4 w-4" />}
      </button>
      {open ? (
        <div className="max-h-[min(420px,50vh)] overflow-auto border-t px-2 pb-2">
          <table className="w-full text-xs">
            <thead className="sticky top-0 bg-muted/80 backdrop-blur-sm">
              <tr className="text-left text-muted-foreground">
                <th className="p-2 font-medium">Alan</th>
                <th className="p-2 font-medium">Açıklama</th>
                <th className="p-2 font-medium">Örnek</th>
                <th className="p-2 font-medium">Op</th>
              </tr>
            </thead>
            <tbody>
              {CUSTOM_CONDITION_FIELD_REFERENCE.map((row) => (
                <tr key={row.field} className="border-t border-border/60 align-top">
                  <td className="p-2">
                    <div className="flex items-center gap-1">
                      <code className="rounded bg-background px-1 py-0.5 font-mono text-[11px]">
                        {row.field}
                      </code>
                      <Button
                        type="button"
                        variant="ghost"
                        size="icon"
                        className="h-6 w-6 shrink-0"
                        title="Kopyala"
                        onClick={() => void copyField(row.field)}
                      >
                        <Copy className="h-3 w-3" />
                      </Button>
                    </div>
                    <div className="mt-0.5 text-muted-foreground">{row.label}</div>
                  </td>
                  <td className="p-2 text-muted-foreground">{row.description}</td>
                  <td className="p-2">
                    <code className="font-mono text-[11px]">{row.exampleValue}</code>
                  </td>
                  <td className="p-2">
                    <div className="flex flex-wrap gap-0.5">
                      {row.operators.map((op) => (
                        <Badge key={op} variant="outline" className="font-mono text-[10px] px-1">
                          {op}
                        </Badge>
                      ))}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : null}
    </div>
  )
}
