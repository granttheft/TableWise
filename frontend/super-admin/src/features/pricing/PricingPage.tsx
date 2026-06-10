import { useState } from 'react'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import { Loader2, Save } from 'lucide-react'
import { toast } from 'sonner'
import api from '@/lib/api'
import { useAuth } from '@/hooks/useAuth'
import { Button } from '@/components/ui/button'
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import { Skeleton } from '@/components/ui/skeleton'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { Switch } from '@/components/ui/switch'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'

interface PlanLimits {
  maxVenues: number
  maxTables: number
  maxRules: number
  maxReservationsPerMonth: number
  maxStaffAccounts: number
}

interface PlanFeatures {
  depositEnabled: boolean
  signalREnabled: boolean
  apiEnabled: boolean
  customWhatsAppEnabled: boolean
  analyticsEnabled: boolean
}

interface PlanPricing {
  id: string
  name: string
  tier: string
  monthlyPriceTry: number
  yearlyPriceTry: number
  isVisible: boolean
  limitsJson: string
  featuresJson: string
}

const DEFAULT_LIMITS: PlanLimits = {
  maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3,
}
const DEFAULT_FEATURES: PlanFeatures = {
  depositEnabled: false, signalREnabled: true, apiEnabled: false,
  customWhatsAppEnabled: false, analyticsEnabled: false,
}

function parseLimits(json: string): PlanLimits {
  try { return { ...DEFAULT_LIMITS, ...JSON.parse(json) } } catch { return DEFAULT_LIMITS }
}

function parseFeatures(json: string): PlanFeatures {
  try { return { ...DEFAULT_FEATURES, ...JSON.parse(json) } } catch { return DEFAULT_FEATURES }
}

function displayLimit(val: number): string {
  return val === -1 ? 'Sınırsız' : String(val)
}

const TIER_COLORS: Record<string, string> = {
  Starter:    'border-zinc-500/30 bg-zinc-500/5',
  Pro:        'border-indigo-500/30 bg-indigo-500/5',
  Business:   'border-violet-500/30 bg-violet-500/5',
  Enterprise: 'border-amber-500/30 bg-amber-500/5',
}

const LIMIT_LABELS: { key: keyof PlanLimits; label: string }[] = [
  { key: 'maxVenues',               label: 'Mekan Sayısı' },
  { key: 'maxTables',               label: 'Masa Sayısı' },
  { key: 'maxRules',                label: 'Kural Sayısı' },
  { key: 'maxReservationsPerMonth', label: 'Aylık Rezervasyon' },
  { key: 'maxStaffAccounts',        label: 'Personel Hesabı' },
]

const FEATURE_LABELS: { key: keyof PlanFeatures; label: string; tooltip?: string }[] = [
  { key: 'depositEnabled',        label: 'Kapora Modülü',              tooltip: 'İsteğe bağlı eklenti' },
  { key: 'signalREnabled',        label: 'Gerçek Zamanlı Takip' },
  { key: 'apiEnabled',            label: 'API Erişimi' },
  { key: 'customWhatsAppEnabled', label: 'Kendi WhatsApp Numarası' },
  { key: 'analyticsEnabled',      label: 'Gelişmiş Analitik' },
]

interface PlanEditState {
  monthly: string
  yearly: string
  limits: PlanLimits
  features: PlanFeatures
}

export function PricingPage() {
  const queryClient = useQueryClient()
  const { isSuperAdmin, isFinance } = useAuth()
  const canEdit = isSuperAdmin || isFinance

  const { data: plans, isLoading } = useQuery<PlanPricing[]>({
    queryKey: ['platform-pricing'],
    queryFn: () => api.get('/api/platform/pricing').then((r) => r.data),
    staleTime: 60_000,
  })

  const [edits, setEdits] = useState<Record<string, PlanEditState>>({})
  const [saving, setSaving] = useState<string | null>(null)

  function getEdit(plan: PlanPricing): PlanEditState {
    return edits[plan.id] ?? {
      monthly:  String(plan.monthlyPriceTry),
      yearly:   String(plan.yearlyPriceTry),
      limits:   parseLimits(plan.limitsJson),
      features: parseFeatures(plan.featuresJson),
    }
  }

  function patchEdit(id: string, patch: Partial<PlanEditState>, plan: PlanPricing) {
    setEdits((prev) => ({
      ...prev,
      [id]: { ...getEdit(plan), ...prev[id], ...patch },
    }))
  }

  function isDirty(plan: PlanPricing) {
    return plan.id in edits
  }

  async function handleSave(plan: PlanPricing) {
    const edit = getEdit(plan)
    const monthly = parseFloat(edit.monthly)
    const yearly = parseFloat(edit.yearly)
    if (isNaN(monthly) || isNaN(yearly) || monthly < 0 || yearly < 0) {
      toast.error('Geçerli bir fiyat girin.')
      return
    }
    try {
      setSaving(plan.id)
      await api.put(`/api/platform/pricing/${plan.id}`, {
        monthlyPriceTry: monthly,
        yearlyPriceTry:  yearly,
        limitsJson:   JSON.stringify(edit.limits),
        featuresJson: JSON.stringify(edit.features),
      })
      toast.success(`${plan.name} planı güncellendi.`)
      setEdits((prev) => {
        const next = { ...prev }
        delete next[plan.id]
        return next
      })
      queryClient.invalidateQueries({ queryKey: ['platform-pricing'] })
    } catch {
      toast.error('Plan güncellenirken hata oluştu.')
    } finally {
      setSaving(null)
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-72" />)}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Fiyatlandırma</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Platform planlarının fiyatlarını, limitlerini ve özellik flag'lerini yönetin.
        </p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {plans?.map((plan) => {
          const isEnterprise = plan.tier === 'Enterprise'
          const edit = getEdit(plan)
          const colorClass = TIER_COLORS[plan.tier] ?? 'border-border'

          return (
            <Card key={plan.id} className={`border ${colorClass}`}>
              <CardHeader className="pb-2">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-base">{plan.name}</CardTitle>
                  <span className="text-xs text-muted-foreground">{plan.tier}</span>
                </div>
              </CardHeader>
              <CardContent>
                <Tabs defaultValue="prices">
                  <TabsList className="w-full mb-3 h-8">
                    <TabsTrigger value="prices"   className="flex-1 text-xs">Fiyatlar</TabsTrigger>
                    <TabsTrigger value="limits"   className="flex-1 text-xs">Limitler</TabsTrigger>
                    <TabsTrigger value="features" className="flex-1 text-xs">Özellikler</TabsTrigger>
                  </TabsList>

                  {/* Fiyatlar */}
                  <TabsContent value="prices" className="space-y-3 mt-0">
                    {isEnterprise ? (
                      <div className="text-center py-4">
                        <p className="text-sm font-medium">Özel Teklif</p>
                        <p className="text-xs text-muted-foreground mt-1">Fiyat müzakereye bağlı</p>
                      </div>
                    ) : (
                      <>
                        <div className="space-y-1">
                          <label className="text-xs text-muted-foreground">Aylık (₺)</label>
                          <Input
                            type="number" min="0"
                            value={edit.monthly}
                            onChange={(e) => patchEdit(plan.id, { monthly: e.target.value }, plan)}
                            disabled={!canEdit}
                            className="h-8 text-sm"
                          />
                        </div>
                        <div className="space-y-1">
                          <label className="text-xs text-muted-foreground">Yıllık (₺)</label>
                          <Input
                            type="number" min="0"
                            value={edit.yearly}
                            onChange={(e) => patchEdit(plan.id, { yearly: e.target.value }, plan)}
                            disabled={!canEdit}
                            className="h-8 text-sm"
                          />
                        </div>
                      </>
                    )}
                  </TabsContent>

                  {/* Limitler */}
                  <TabsContent value="limits" className="space-y-3 mt-0">
                    {LIMIT_LABELS.map(({ key, label }) => {
                      const isUnlimited = edit.limits[key] === -1
                      return (
                        <div key={key} className="space-y-1">
                          <label className="text-xs text-muted-foreground">{label}</label>
                          <div className="flex items-center gap-2">
                            <Input
                              type="number" min="0"
                              value={isUnlimited ? '' : displayLimit(edit.limits[key])}
                              placeholder={isUnlimited ? 'Sınırsız' : ''}
                              disabled={!canEdit || isUnlimited}
                              onChange={(e) => {
                                const val = parseInt(e.target.value)
                                if (!isNaN(val)) {
                                  patchEdit(plan.id, { limits: { ...edit.limits, [key]: val } }, plan)
                                }
                              }}
                              className="h-7 text-xs flex-1"
                            />
                            <div className="flex items-center gap-1">
                              <Checkbox
                                id={`${plan.id}-${key}-unlimited`}
                                checked={isUnlimited}
                                disabled={!canEdit}
                                onCheckedChange={(checked: boolean) => {
                                  patchEdit(plan.id, {
                                    limits: { ...edit.limits, [key]: checked ? -1 : 0 },
                                  }, plan)
                                }}
                              />
                              <Label htmlFor={`${plan.id}-${key}-unlimited`} className="text-[10px] text-muted-foreground cursor-pointer">∞</Label>
                            </div>
                          </div>
                        </div>
                      )
                    })}
                  </TabsContent>

                  {/* Özellikler */}
                  <TabsContent value="features" className="space-y-3 mt-0">
                    {FEATURE_LABELS.map(({ key, label, tooltip }) => (
                      <div key={key} className="flex items-center justify-between">
                        <div>
                          <span className="text-xs text-foreground">{label}</span>
                          {tooltip && <p className="text-[10px] text-muted-foreground">{tooltip}</p>}
                        </div>
                        <Switch
                          checked={edit.features[key]}
                          disabled={!canEdit}
                          onCheckedChange={(checked) => {
                            patchEdit(plan.id, { features: { ...edit.features, [key]: checked } }, plan)
                          }}
                        />
                      </div>
                    ))}
                  </TabsContent>
                </Tabs>

                {canEdit && (
                  <Button
                    size="sm"
                    className="w-full mt-3"
                    disabled={!isDirty(plan) || saving === plan.id}
                    onClick={() => handleSave(plan)}
                  >
                    {saving === plan.id
                      ? <Loader2 className="mr-2 h-3 w-3 animate-spin" />
                      : <Save className="mr-2 h-3 w-3" />}
                    Kaydet
                  </Button>
                )}
              </CardContent>
            </Card>
          )
        })}
      </div>

      {!canEdit && (
        <p className="text-sm text-muted-foreground">
          Düzenlemek için SuperAdmin veya Finance rolü gereklidir.
        </p>
      )}
    </div>
  )
}
