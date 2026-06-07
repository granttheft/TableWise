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

interface PlanPricing {
  id: string
  name: string
  tier: string
  monthlyPriceTry: number
  yearlyPriceTry: number
  isVisible: boolean
}

const TIER_COLORS: Record<string, string> = {
  Starter: 'border-zinc-500/30 bg-zinc-500/5',
  Pro: 'border-indigo-500/30 bg-indigo-500/5',
  Business: 'border-violet-500/30 bg-violet-500/5',
  Enterprise: 'border-amber-500/30 bg-amber-500/5',
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

  const [edits, setEdits] = useState<Record<string, { monthly: string; yearly: string }>>({})
  const [saving, setSaving] = useState<string | null>(null)

  function getMonthly(plan: PlanPricing) {
    return edits[plan.id]?.monthly ?? String(plan.monthlyPriceTry)
  }

  function getYearly(plan: PlanPricing) {
    return edits[plan.id]?.yearly ?? String(plan.yearlyPriceTry)
  }

  function setMonthly(id: string, val: string) {
    setEdits((prev) => ({ ...prev, [id]: { ...prev[id], monthly: val } }))
  }

  function setYearly(id: string, val: string) {
    setEdits((prev) => ({ ...prev, [id]: { ...prev[id], yearly: val } }))
  }

  async function handleSave(plan: PlanPricing) {
    const monthly = parseFloat(getMonthly(plan))
    const yearly = parseFloat(getYearly(plan))
    if (isNaN(monthly) || isNaN(yearly) || monthly < 0 || yearly < 0) {
      toast.error('Geçerli bir fiyat girin.')
      return
    }
    try {
      setSaving(plan.id)
      await api.put(`/api/platform/pricing/${plan.id}`, {
        monthlyPriceTry: monthly,
        yearlyPriceTry: yearly,
      })
      toast.success(`${plan.name} planı fiyatı güncellendi.`)
      setEdits((prev) => {
        const next = { ...prev }
        delete next[plan.id]
        return next
      })
      queryClient.invalidateQueries({ queryKey: ['platform-pricing'] })
    } catch {
      toast.error('Fiyat güncellenirken hata oluştu.')
    } finally {
      setSaving(null)
    }
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        <Skeleton className="h-8 w-48" />
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
          {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-48" />)}
        </div>
      </div>
    )
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold">Fiyatlandırma</h1>
        <p className="text-sm text-muted-foreground mt-1">Platform planlarının aylık ve yıllık fiyatlarını yönetin.</p>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {plans?.map((plan) => {
          const isEnterprise = plan.tier === 'Enterprise'
          const isDirty = edits[plan.id] !== undefined
          const colorClass = TIER_COLORS[plan.tier] ?? 'border-border'

          return (
            <Card key={plan.id} className={`border ${colorClass}`}>
              <CardHeader className="pb-3">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-base">{plan.name}</CardTitle>
                  <span className="text-xs text-muted-foreground">{plan.tier}</span>
                </div>
              </CardHeader>
              <CardContent className="space-y-4">
                {isEnterprise ? (
                  <div className="text-center py-4">
                    <p className="text-sm font-medium">Özel Teklif</p>
                    <p className="text-xs text-muted-foreground mt-1">Fiyat müzakereye bağlı</p>
                  </div>
                ) : (
                  <>
                    <div className="space-y-2">
                      <label className="text-xs text-muted-foreground">Aylık Fiyat (₺)</label>
                      <Input
                        type="number"
                        min="0"
                        value={getMonthly(plan)}
                        onChange={(e) => setMonthly(plan.id, e.target.value)}
                        disabled={!canEdit}
                        className="h-8 text-sm"
                      />
                    </div>
                    <div className="space-y-2">
                      <label className="text-xs text-muted-foreground">Yıllık Fiyat (₺)</label>
                      <Input
                        type="number"
                        min="0"
                        value={getYearly(plan)}
                        onChange={(e) => setYearly(plan.id, e.target.value)}
                        disabled={!canEdit}
                        className="h-8 text-sm"
                      />
                    </div>
                    {canEdit && (
                      <Button
                        size="sm"
                        className="w-full"
                        disabled={!isDirty || saving === plan.id}
                        onClick={() => handleSave(plan)}
                      >
                        {saving === plan.id
                          ? <Loader2 className="mr-2 h-3 w-3 animate-spin" />
                          : <Save className="mr-2 h-3 w-3" />}
                        Kaydet
                      </Button>
                    )}
                  </>
                )}
              </CardContent>
            </Card>
          )
        })}
      </div>

      {!canEdit && (
        <p className="text-sm text-muted-foreground">
          Fiyat düzenlemek için SuperAdmin veya Finance rolü gereklidir.
        </p>
      )}
    </div>
  )
}
