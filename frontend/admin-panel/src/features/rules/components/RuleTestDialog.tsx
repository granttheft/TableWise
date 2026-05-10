import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Badge } from '@/components/ui/badge'
import { Card, CardContent } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Switch } from '@/components/ui/switch'
import { CheckCircle, XCircle, Clock, Play } from 'lucide-react'
import { useTestRule } from '@/hooks/useRules'
import type { RuleTestContext, RuleTestResult } from '@/types/api'
import { dayLabels } from '../data/ruleTemplates'

const testContextSchema = z.object({
  partySize: z.number().min(1).max(50),
  daysInAdvance: z.number().min(0).max(365),
  customerTier: z.enum(['Regular', 'Vip', 'Blacklisted']),
  dayOfWeek: z.number().min(0).max(6),
  hour: z.number().min(0).max(23),
  occupancyPercent: z.number().min(0).max(100),
  hasChildren: z.boolean().optional(),
  childCount: z.number().min(0).max(20).optional(),
})

type TestContextForm = z.infer<typeof testContextSchema>

interface RuleTestDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  ruleId: string
  ruleName: string
}

export function RuleTestDialog({
  open,
  onOpenChange,
  ruleId,
  ruleName,
}: RuleTestDialogProps) {
  const [result, setResult] = useState<RuleTestResult | null>(null)
  const testRuleMutation = useTestRule()

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
  } = useForm<TestContextForm>({
    resolver: zodResolver(testContextSchema),
    defaultValues: {
      partySize: 4,
      daysInAdvance: 3,
      customerTier: 'Regular',
      dayOfWeek: new Date().getDay(),
      hour: 19,
      occupancyPercent: 50,
      hasChildren: false,
      childCount: 0,
    },
  })

  const hasChildren = watch('hasChildren')

  const onSubmit = async (data: TestContextForm) => {
    setResult(null)
    try {
      const testResult = await testRuleMutation.mutateAsync({
        ruleId,
        context: data as RuleTestContext,
      })
      setResult(testResult)
    } catch (error) {
      // Error handled by mutation
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[85vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Kural Testi</DialogTitle>
          <DialogDescription>
            "{ruleName}" kuralını örnek verilerle test edin
          </DialogDescription>
        </DialogHeader>

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
          <div className="grid gap-4 md:grid-cols-2">
            <div className="space-y-2">
              <Label htmlFor="partySize">Kişi Sayısı</Label>
              <Input
                id="partySize"
                type="number"
                min={1}
                max={50}
                {...register('partySize', { valueAsNumber: true })}
              />
              {errors.partySize && (
                <p className="text-sm text-destructive">{errors.partySize.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="daysInAdvance">Kaç Gün Öncesi</Label>
              <Input
                id="daysInAdvance"
                type="number"
                min={0}
                max={365}
                {...register('daysInAdvance', { valueAsNumber: true })}
              />
              {errors.daysInAdvance && (
                <p className="text-sm text-destructive">{errors.daysInAdvance.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label>Müşteri Tipi</Label>
              <Select
                value={watch('customerTier')}
                onValueChange={(value) => setValue('customerTier', value as any)}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Regular">Normal</SelectItem>
                  <SelectItem value="Vip">VIP</SelectItem>
                  <SelectItem value="Blacklisted">Kara Liste</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label>Gün</Label>
              <Select
                value={watch('dayOfWeek').toString()}
                onValueChange={(value) => setValue('dayOfWeek', parseInt(value))}
              >
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(dayLabels).map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="hour">Saat</Label>
              <Input
                id="hour"
                type="number"
                min={0}
                max={23}
                {...register('hour', { valueAsNumber: true })}
              />
              {errors.hour && (
                <p className="text-sm text-destructive">{errors.hour.message}</p>
              )}
            </div>

            <div className="space-y-2">
              <Label htmlFor="occupancyPercent">Doluluk Oranı (%)</Label>
              <Input
                id="occupancyPercent"
                type="number"
                min={0}
                max={100}
                {...register('occupancyPercent', { valueAsNumber: true })}
              />
              {errors.occupancyPercent && (
                <p className="text-sm text-destructive">{errors.occupancyPercent.message}</p>
              )}
            </div>
          </div>

          <div className="space-y-4 border-t pt-4">
            <div className="flex items-center justify-between">
              <div className="space-y-0.5">
                <Label>Çocuklu Grup</Label>
                <p className="text-sm text-muted-foreground">
                  Grupta çocuk var mı?
                </p>
              </div>
              <Switch
                checked={hasChildren}
                onCheckedChange={(checked) => setValue('hasChildren', checked)}
              />
            </div>

            {hasChildren && (
              <div className="space-y-2">
                <Label htmlFor="childCount">Çocuk Sayısı</Label>
                <Input
                  id="childCount"
                  type="number"
                  min={0}
                  max={20}
                  {...register('childCount', { valueAsNumber: true })}
                />
              </div>
            )}
          </div>

          {/* Test Result */}
          {result && (
            <Card className={result.triggered ? 'border-green-500' : 'border-muted'}>
              <CardContent className="p-4">
                <div className="flex items-center justify-between mb-3">
                  <div className="flex items-center space-x-2">
                    {result.triggered ? (
                      <CheckCircle className="h-5 w-5 text-green-500" />
                    ) : (
                      <XCircle className="h-5 w-5 text-muted-foreground" />
                    )}
                    <span className="font-semibold">
                      {result.triggered ? 'Kural Tetiklendi' : 'Kural Tetiklenmedi'}
                    </span>
                  </div>
                  <Badge variant="secondary" className="flex items-center space-x-1">
                    <Clock className="h-3 w-3" />
                    <span>{result.executionTimeMs}ms</span>
                  </Badge>
                </div>

                {result.outcome && (
                  <div className="space-y-2 text-sm">
                    <div className="flex items-center justify-between">
                      <span className="text-muted-foreground">Aksiyon:</span>
                      <Badge>{result.outcome.action}</Badge>
                    </div>
                    {result.outcome.message && (
                      <div>
                        <span className="text-muted-foreground">Mesaj: </span>
                        <span>{result.outcome.message}</span>
                      </div>
                    )}
                    {result.outcome.payload && (
                      <div>
                        <span className="text-muted-foreground">Ek Bilgi:</span>
                        <pre className="mt-1 p-2 bg-muted rounded text-xs overflow-auto">
                          {JSON.stringify(result.outcome.payload, null, 2)}
                        </pre>
                      </div>
                    )}
                  </div>
                )}

                {result.evaluationPath && result.evaluationPath.length > 0 && (
                  <div className="mt-3 pt-3 border-t">
                    <span className="text-sm text-muted-foreground">Değerlendirme Yolu:</span>
                    <div className="flex flex-wrap gap-1 mt-1">
                      {result.evaluationPath.map((step, index) => (
                        <Badge key={index} variant="outline" className="text-xs">
                          {step}
                        </Badge>
                      ))}
                    </div>
                  </div>
                )}
              </CardContent>
            </Card>
          )}

          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
            >
              Kapat
            </Button>
            <Button type="submit" disabled={testRuleMutation.isPending}>
              <Play className="h-4 w-4 mr-2" />
              {testRuleMutation.isPending ? 'Test ediliyor...' : 'Test Et'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
