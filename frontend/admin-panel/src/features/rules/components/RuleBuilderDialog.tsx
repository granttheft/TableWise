import { useState, useEffect } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import * as z from 'zod'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Textarea } from '@/components/ui/textarea'
import { Card, CardContent } from '@/components/ui/card'
import { Badge } from '@/components/ui/badge'
import { Switch } from '@/components/ui/switch'
import { Slider } from '@/components/ui/slider'
import { Checkbox } from '@/components/ui/checkbox'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  ChevronLeft,
  ChevronRight,
  Check,
  Play,
  Ban,
  AlertTriangle,
  Lightbulb,
  Percent,
  CreditCard,
  ExternalLink,
  Plus,
  Trash2,
  Code,
} from 'lucide-react'
import type { Rule, RuleTemplate, RuleCondition, RuleAction, ActionType, ConditionOperator, RuleTrigger } from '@/types/api'
import { conditionFields, operatorLabels, dayLabels, actionTypeLabels } from '../data/ruleTemplates'
import { ruleToHumanReadable, getPriorityDescription, getPriorityColor } from '../utils/ruleHumanReadable'
import { RuleTestDialog } from './RuleTestDialog'
import { cn } from '@/lib/cn'

const actionIcons: Record<string, any> = {
  Block: Ban,
  Warn: AlertTriangle,
  Suggest: Lightbulb,
  Discount: Percent,
  Deposit: CreditCard,
  Redirect: ExternalLink,
}

const ruleFormSchema = z.object({
  name: z.string().min(1, 'Kural adı zorunludur').max(100),
  description: z.string().max(500).optional(),
  priority: z.number().min(1).max(10),
  trigger: z.enum(['OnReservation', 'OnSeating', 'OnModification', 'OnCancellation']),
  isActive: z.boolean(),
})

type RuleFormData = z.infer<typeof ruleFormSchema>

interface RuleBuilderDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  template?: RuleTemplate | null
  existingRule?: Rule | null
  venueId: string
  onSubmit: (rule: any) => void
  isLoading?: boolean
}

const STEPS = [
  { id: 1, title: 'Temel Bilgiler', description: 'İsim, açıklama ve öncelik' },
  { id: 2, title: 'Koşullar', description: 'Kuralın tetiklenme koşulları' },
  { id: 3, title: 'Aksiyonlar', description: 'Kural tetiklenince yapılacaklar' },
  { id: 4, title: 'Önizleme', description: 'Özet ve test' },
]

export function RuleBuilderDialog({
  open,
  onOpenChange,
  template,
  existingRule,
  venueId,
  onSubmit,
  isLoading,
}: RuleBuilderDialogProps) {
  const [currentStep, setCurrentStep] = useState(1)
  const [conditions, setConditions] = useState<RuleCondition[]>([])
  const [actions, setActions] = useState<RuleAction[]>([])
  const [showJsonEditor, setShowJsonEditor] = useState(false)
  const [showTestDialog, setShowTestDialog] = useState(false)
  const [tempRuleId, setTempRuleId] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
    setValue,
    watch,
    reset,
  } = useForm<RuleFormData>({
    resolver: zodResolver(ruleFormSchema),
    defaultValues: {
      name: '',
      description: '',
      priority: 5,
      trigger: 'OnReservation',
      isActive: true,
    },
  })

  const priority = watch('priority')
  const trigger = watch('trigger')

  // Initialize from template or existing rule
  useEffect(() => {
    if (template) {
      reset({
        name: template.name,
        description: template.description,
        priority: 5,
        trigger: 'OnReservation',
        isActive: true,
      })
      setConditions(template.defaultConditions?.conditions || [])
      setActions(template.defaultActions?.map(a => a as RuleAction) || [])
    } else if (existingRule) {
      reset({
        name: existingRule.name,
        description: existingRule.description || '',
        priority: existingRule.priority,
        trigger: existingRule.trigger,
        isActive: existingRule.isActive,
      })
      setConditions(existingRule.conditions?.conditions || [])
      setActions(existingRule.actions || [])
      setTempRuleId(existingRule.id)
    } else {
      reset({
        name: '',
        description: '',
        priority: 5,
        trigger: 'OnReservation',
        isActive: true,
      })
      setConditions([])
      setActions([])
    }
    setCurrentStep(1)
  }, [template, existingRule, reset, open])

  const handleNext = () => {
    if (currentStep < 4) setCurrentStep(currentStep + 1)
  }

  const handleBack = () => {
    if (currentStep > 1) setCurrentStep(currentStep - 1)
  }

  const addCondition = () => {
    setConditions([
      ...conditions,
      {
        field: 'PartySize',
        operator: 'GreaterThanOrEquals',
        value: 4,
        logicalOperator: conditions.length > 0 ? 'And' : undefined,
      },
    ])
  }

  const updateCondition = (index: number, updates: Partial<RuleCondition>) => {
    const newConditions = [...conditions]
    newConditions[index] = { ...newConditions[index], ...updates }
    setConditions(newConditions)
  }

  const removeCondition = (index: number) => {
    setConditions(conditions.filter((_, i) => i !== index))
  }

  const addAction = (type: ActionType) => {
    const newAction: RuleAction = { type }
    switch (type) {
      case 'Block':
      case 'Warn':
        newAction.message = ''
        break
      case 'Discount':
        newAction.discountPercent = 10
        break
      case 'Deposit':
        newAction.depositAmount = 100
        newAction.depositPerPerson = false
        break
    }
    setActions([...actions, newAction])
  }

  const updateAction = (index: number, updates: Partial<RuleAction>) => {
    const newActions = [...actions]
    newActions[index] = { ...newActions[index], ...updates }
    setActions(newActions)
  }

  const removeAction = (index: number) => {
    setActions(actions.filter((_, i) => i !== index))
  }

  const handleFormSubmit = (data: RuleFormData) => {
    const ruleData = {
      ...data,
      venueId,
      ruleType: template?.ruleType || 'CustomCondition',
      conditions: {
        conditions,
        logicalOperator: 'And' as const,
      },
      actions,
    }
    onSubmit(ruleData)
  }

  const previewRule: Partial<Rule> = {
    name: watch('name'),
    description: watch('description'),
    priority: watch('priority'),
    trigger: watch('trigger'),
    conditions: { conditions, logicalOperator: 'And' },
    actions,
  }

  const humanReadableText = ruleToHumanReadable(previewRule as Rule)

  return (
    <>
      <Dialog open={open} onOpenChange={onOpenChange}>
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-hidden flex flex-col">
          <DialogHeader>
            <DialogTitle>
              {existingRule ? 'Kuralı Düzenle' : template ? `${template.name} Kuralı` : 'Yeni Kural'}
            </DialogTitle>
            <DialogDescription>
              Adım {currentStep} / {STEPS.length}: {STEPS[currentStep - 1].description}
            </DialogDescription>
          </DialogHeader>

          {/* Step Indicator */}
          <div className="flex items-center justify-between px-2 py-4 border-b">
            {STEPS.map((step, index) => (
              <div key={step.id} className="flex items-center">
                <div
                  className={cn(
                    'flex items-center justify-center w-8 h-8 rounded-full text-sm font-medium transition-colors',
                    currentStep >= step.id
                      ? 'bg-accent text-white'
                      : 'bg-muted text-muted-foreground'
                  )}
                >
                  {currentStep > step.id ? <Check className="h-4 w-4" /> : step.id}
                </div>
                {index < STEPS.length - 1 && (
                  <div
                    className={cn(
                      'w-16 h-1 mx-2 rounded',
                      currentStep > step.id ? 'bg-accent' : 'bg-muted'
                    )}
                  />
                )}
              </div>
            ))}
          </div>

          {/* Step Content */}
          <div className="flex-1 overflow-y-auto py-4">
            {/* Step 1: Basic Info */}
            {currentStep === 1 && (
              <div className="space-y-6 px-1">
                <div className="space-y-2">
                  <Label htmlFor="name">Kural Adı *</Label>
                  <Input
                    id="name"
                    placeholder="Örn: Hafta Sonu VIP İndirimi"
                    {...register('name')}
                  />
                  {errors.name && (
                    <p className="text-sm text-destructive">{errors.name.message}</p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Açıklama</Label>
                  <Textarea
                    id="description"
                    placeholder="Bu kuralın ne yaptığını kısaca açıklayın"
                    rows={3}
                    {...register('description')}
                  />
                </div>

                <div className="space-y-3">
                  <div className="flex items-center justify-between">
                    <Label>Öncelik: {priority}</Label>
                    <Badge className={getPriorityColor(priority)}>{priority}</Badge>
                  </div>
                  <Slider
                    value={[priority]}
                    onValueChange={(value) => setValue('priority', value[0])}
                    min={1}
                    max={10}
                    step={1}
                  />
                  <p className="text-sm text-muted-foreground">
                    {getPriorityDescription(priority)}
                  </p>
                </div>

                <div className="space-y-3">
                  <Label>Tetiklenme Zamanı</Label>
                  <div className="grid grid-cols-2 gap-3">
                    {[
                      { value: 'OnReservation', label: 'Rezervasyonda', desc: 'Rezervasyon oluşturulurken' },
                      { value: 'OnSeating', label: 'Oturmada', desc: 'Müşteri masaya oturtulurken' },
                      { value: 'OnModification', label: 'Değişiklikte', desc: 'Rezervasyon değiştirilirken' },
                      { value: 'OnCancellation', label: 'İptalde', desc: 'Rezervasyon iptal edilirken' },
                    ].map((option) => (
                      <Card
                        key={option.value}
                        className={cn(
                          'cursor-pointer transition-all',
                          trigger === option.value && 'border-accent ring-2 ring-accent/20'
                        )}
                        onClick={() => setValue('trigger', option.value as RuleTrigger)}
                      >
                        <CardContent className="p-3">
                          <div className="font-medium text-sm">{option.label}</div>
                          <div className="text-xs text-muted-foreground">{option.desc}</div>
                        </CardContent>
                      </Card>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Step 2: Conditions */}
            {currentStep === 2 && (
              <div className="space-y-4 px-1">
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="font-medium">Koşullar</h3>
                    <p className="text-sm text-muted-foreground">
                      Kuralın tetiklenmesi için gerekli koşulları belirleyin
                    </p>
                  </div>
                  <div className="flex items-center space-x-2">
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={() => setShowJsonEditor(!showJsonEditor)}
                    >
                      <Code className="h-4 w-4 mr-1" />
                      {showJsonEditor ? 'Form' : 'JSON'}
                    </Button>
                    <Button type="button" size="sm" onClick={addCondition}>
                      <Plus className="h-4 w-4 mr-1" />
                      Koşul Ekle
                    </Button>
                  </div>
                </div>

                {showJsonEditor ? (
                  <div className="space-y-2">
                    <Label>JSON Koşulları</Label>
                    <Textarea
                      value={JSON.stringify(conditions, null, 2)}
                      onChange={(e) => {
                        try {
                          setConditions(JSON.parse(e.target.value))
                        } catch {}
                      }}
                      rows={10}
                      className="font-mono text-sm"
                    />
                  </div>
                ) : (
                  <div className="space-y-3">
                    {conditions.length === 0 ? (
                      <Card className="border-dashed">
                        <CardContent className="p-6 text-center text-muted-foreground">
                          <p>Henüz koşul eklenmedi.</p>
                          <p className="text-sm">Koşulsuz kurallar her zaman tetiklenir.</p>
                        </CardContent>
                      </Card>
                    ) : (
                      conditions.map((condition, index) => (
                        <Card key={index}>
                          <CardContent className="p-4">
                            <div className="flex items-start gap-3">
                              {index > 0 && (
                                <Select
                                  value={condition.logicalOperator || 'And'}
                                  onValueChange={(value) =>
                                    updateCondition(index, { logicalOperator: value as any })
                                  }
                                >
                                  <SelectTrigger className="w-20">
                                    <SelectValue />
                                  </SelectTrigger>
                                  <SelectContent>
                                    <SelectItem value="And">VE</SelectItem>
                                    <SelectItem value="Or">VEYA</SelectItem>
                                  </SelectContent>
                                </Select>
                              )}

                              <Select
                                value={condition.field}
                                onValueChange={(value) => updateCondition(index, { field: value })}
                              >
                                <SelectTrigger className="flex-1">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  {conditionFields.map((field) => (
                                    <SelectItem key={field.value} value={field.value}>
                                      {field.label}
                                    </SelectItem>
                                  ))}
                                </SelectContent>
                              </Select>

                              <Select
                                value={condition.operator}
                                onValueChange={(value) =>
                                  updateCondition(index, { operator: value as ConditionOperator })
                                }
                              >
                                <SelectTrigger className="w-40">
                                  <SelectValue />
                                </SelectTrigger>
                                <SelectContent>
                                  {Object.entries(operatorLabels).map(([value, label]) => (
                                    <SelectItem key={value} value={value}>
                                      {label}
                                    </SelectItem>
                                  ))}
                                </SelectContent>
                              </Select>

                              {condition.field === 'DayOfWeek' ? (
                                <div className="flex flex-wrap gap-1">
                                  {Object.entries(dayLabels).map(([day, label]) => (
                                    <Badge
                                      key={day}
                                      variant={
                                        (Array.isArray(condition.value) && condition.value.includes(parseInt(day)))
                                          ? 'default'
                                          : 'outline'
                                      }
                                      className="cursor-pointer"
                                      onClick={() => {
                                        const current = Array.isArray(condition.value) ? condition.value : []
                                        const dayNum = parseInt(day)
                                        const newValue = current.includes(dayNum)
                                          ? current.filter((d) => d !== dayNum)
                                          : [...current, dayNum]
                                        updateCondition(index, { value: newValue })
                                      }}
                                    >
                                      {label.slice(0, 3)}
                                    </Badge>
                                  ))}
                                </div>
                              ) : condition.field === 'CustomerTier' ? (
                                <Select
                                  value={condition.value as string}
                                  onValueChange={(value) => updateCondition(index, { value })}
                                >
                                  <SelectTrigger className="w-32">
                                    <SelectValue />
                                  </SelectTrigger>
                                  <SelectContent>
                                    <SelectItem value="Regular">Normal</SelectItem>
                                    <SelectItem value="Vip">VIP</SelectItem>
                                    <SelectItem value="Blacklisted">Kara Liste</SelectItem>
                                  </SelectContent>
                                </Select>
                              ) : condition.field === 'HasChildren' || condition.field === 'IsWeekend' || condition.field === 'IsHoliday' ? (
                                <Select
                                  value={condition.value?.toString()}
                                  onValueChange={(value) => updateCondition(index, { value: value === 'true' })}
                                >
                                  <SelectTrigger className="w-24">
                                    <SelectValue />
                                  </SelectTrigger>
                                  <SelectContent>
                                    <SelectItem value="true">Evet</SelectItem>
                                    <SelectItem value="false">Hayır</SelectItem>
                                  </SelectContent>
                                </Select>
                              ) : (
                                <Input
                                  type="number"
                                  value={condition.value as number}
                                  onChange={(e) =>
                                    updateCondition(index, { value: parseInt(e.target.value) || 0 })
                                  }
                                  className="w-24"
                                />
                              )}

                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                onClick={() => removeCondition(index)}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </div>
                          </CardContent>
                        </Card>
                      ))
                    )}
                  </div>
                )}
              </div>
            )}

            {/* Step 3: Actions */}
            {currentStep === 3 && (
              <div className="space-y-4 px-1">
                <div>
                  <h3 className="font-medium">Aksiyonlar</h3>
                  <p className="text-sm text-muted-foreground">
                    Kural tetiklendiğinde yapılacak işlemleri seçin
                  </p>
                </div>

                {actions.length === 0 ? (
                  <div className="grid grid-cols-2 gap-3">
                    {Object.entries(actionTypeLabels).map(([type, info]) => {
                      const Icon = actionIcons[type]
                      return (
                        <Card
                          key={type}
                          className="cursor-pointer transition-all hover:border-accent hover:shadow-sm"
                          onClick={() => addAction(type as ActionType)}
                        >
                          <CardContent className="p-4 flex items-center space-x-3">
                            <div className="p-2 rounded-full bg-accent/10 text-accent">
                              <Icon className="h-5 w-5" />
                            </div>
                            <div>
                              <div className="font-medium text-sm">{info.label}</div>
                              <div className="text-xs text-muted-foreground">{info.description}</div>
                            </div>
                          </CardContent>
                        </Card>
                      )
                    })}
                  </div>
                ) : (
                  <div className="space-y-4">
                    {actions.map((action, index) => {
                      const Icon = actionIcons[action.type]
                      const info = actionTypeLabels[action.type]
                      return (
                        <Card key={index}>
                          <CardContent className="p-4">
                            <div className="flex items-center justify-between mb-3">
                              <div className="flex items-center space-x-2">
                                <div className="p-2 rounded-full bg-accent/10 text-accent">
                                  <Icon className="h-4 w-4" />
                                </div>
                                <div className="font-medium">{info.label}</div>
                              </div>
                              <Button
                                type="button"
                                variant="ghost"
                                size="sm"
                                onClick={() => removeAction(index)}
                              >
                                <Trash2 className="h-4 w-4 text-destructive" />
                              </Button>
                            </div>

                            {(action.type === 'Block' || action.type === 'Warn' || action.type === 'Suggest') && (
                              <div className="space-y-2">
                                <Label>Mesaj</Label>
                                <Input
                                  value={action.message || ''}
                                  onChange={(e) => updateAction(index, { message: e.target.value })}
                                  placeholder="Kullanıcıya gösterilecek mesaj"
                                />
                              </div>
                            )}

                            {action.type === 'Discount' && (
                              <div className="space-y-3">
                                <div className="flex items-center justify-between">
                                  <Label>İndirim Oranı: %{action.discountPercent}</Label>
                                </div>
                                <Slider
                                  value={[action.discountPercent || 10]}
                                  onValueChange={(value) =>
                                    updateAction(index, { discountPercent: value[0] })
                                  }
                                  min={1}
                                  max={50}
                                  step={1}
                                />
                              </div>
                            )}

                            {action.type === 'Deposit' && (
                              <div className="space-y-4">
                                <div className="space-y-2">
                                  <Label>Kapora Tutarı (₺)</Label>
                                  <Input
                                    type="number"
                                    value={action.depositAmount || 100}
                                    onChange={(e) =>
                                      updateAction(index, { depositAmount: parseInt(e.target.value) || 0 })
                                    }
                                    min={10}
                                    max={10000}
                                  />
                                </div>
                                <div className="flex items-center space-x-2">
                                  <Switch
                                    checked={action.depositPerPerson || false}
                                    onCheckedChange={(checked) =>
                                      updateAction(index, { depositPerPerson: checked })
                                    }
                                  />
                                  <Label>Kişi başı hesapla</Label>
                                </div>
                              </div>
                            )}

                            {action.type === 'Redirect' && (
                              <div className="space-y-2">
                                <Label>Yönlendirme URL</Label>
                                <Input
                                  value={action.redirectUrl || ''}
                                  onChange={(e) => updateAction(index, { redirectUrl: e.target.value })}
                                  placeholder="https://..."
                                />
                              </div>
                            )}
                          </CardContent>
                        </Card>
                      )
                    })}

                    <Button
                      type="button"
                      variant="outline"
                      className="w-full"
                      onClick={() => setActions([])}
                    >
                      Başka Aksiyon Seç
                    </Button>
                  </div>
                )}
              </div>
            )}

            {/* Step 4: Preview */}
            {currentStep === 4 && (
              <div className="space-y-6 px-1">
                <Card>
                  <CardContent className="p-4 space-y-4">
                    <div className="flex items-center justify-between">
                      <h3 className="font-semibold text-lg">{watch('name') || 'İsimsiz Kural'}</h3>
                      <Badge className={getPriorityColor(priority)}>Öncelik: {priority}</Badge>
                    </div>

                    {watch('description') && (
                      <p className="text-muted-foreground">{watch('description')}</p>
                    )}

                    <div className="p-4 bg-muted rounded-lg">
                      <h4 className="font-medium mb-2">Kural Özeti</h4>
                      <p className="text-sm">{humanReadableText}</p>
                    </div>

                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Tetiklenme:</span>
                        <Badge variant="outline" className="ml-2">
                          {trigger === 'OnReservation' && 'Rezervasyonda'}
                          {trigger === 'OnSeating' && 'Oturmada'}
                          {trigger === 'OnModification' && 'Değişiklikte'}
                          {trigger === 'OnCancellation' && 'İptalde'}
                        </Badge>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Koşul Sayısı:</span>
                        <Badge variant="outline" className="ml-2">{conditions.length}</Badge>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Aksiyon Sayısı:</span>
                        <Badge variant="outline" className="ml-2">{actions.length}</Badge>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Durum:</span>
                        <Badge variant={watch('isActive') ? 'default' : 'secondary'} className="ml-2">
                          {watch('isActive') ? 'Aktif' : 'Pasif'}
                        </Badge>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <div className="flex items-center space-x-2">
                  <Switch
                    checked={watch('isActive')}
                    onCheckedChange={(checked) => setValue('isActive', checked)}
                  />
                  <Label>Kuralı hemen aktifleştir</Label>
                </div>

                {tempRuleId && (
                  <Button
                    type="button"
                    variant="outline"
                    className="w-full"
                    onClick={() => setShowTestDialog(true)}
                  >
                    <Play className="h-4 w-4 mr-2" />
                    Kuralı Test Et
                  </Button>
                )}
              </div>
            )}
          </div>

          {/* Footer */}
          <div className="flex items-center justify-between pt-4 border-t">
            <Button
              type="button"
              variant="outline"
              onClick={handleBack}
              disabled={currentStep === 1}
            >
              <ChevronLeft className="h-4 w-4 mr-1" />
              Geri
            </Button>

            <div className="flex items-center space-x-2">
              <Button
                type="button"
                variant="ghost"
                onClick={() => onOpenChange(false)}
              >
                İptal
              </Button>

              {currentStep < 4 ? (
                <Button type="button" onClick={handleNext}>
                  İleri
                  <ChevronRight className="h-4 w-4 ml-1" />
                </Button>
              ) : (
                <Button
                  onClick={handleSubmit(handleFormSubmit)}
                  disabled={isLoading || actions.length === 0}
                >
                  {isLoading ? 'Kaydediliyor...' : existingRule ? 'Güncelle' : 'Kuralı Oluştur'}
                </Button>
              )}
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {tempRuleId && (
        <RuleTestDialog
          open={showTestDialog}
          onOpenChange={setShowTestDialog}
          ruleId={tempRuleId}
          ruleName={watch('name')}
        />
      )}
    </>
  )
}
