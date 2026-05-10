import { useState } from 'react'
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Progress } from '@/components/ui/progress'
import { Switch } from '@/components/ui/switch'
import { Label } from '@/components/ui/label'
import { Check, X, CreditCard, Download } from 'lucide-react'
import { format } from 'date-fns'
import { tr } from 'date-fns/locale'
import { toast } from 'sonner'

interface Plan {
  id: string
  name: string
  priceMonthly: number
  priceYearly: number
  features: string[]
  limits: {
    venues: number | 'unlimited'
    tables: number | 'unlimited'
    rules: number | 'unlimited'
    reservationsPerMonth: number | 'unlimited'
  }
}

const plans: Plan[] = [
  {
    id: 'starter',
    name: 'Starter',
    priceMonthly: 490,
    priceYearly: 4900,
    features: [
      '1 mekan',
      '3 masa',
      '5 kural',
      '100 rezervasyon/ay',
      'Email bildirimleri',
      'Temel raporlar',
    ],
    limits: {
      venues: 1,
      tables: 3,
      rules: 5,
      reservationsPerMonth: 100,
    },
  },
  {
    id: 'pro',
    name: 'Pro',
    priceMonthly: 990,
    priceYearly: 9900,
    features: [
      '1 mekan',
      'Sınırsız masa',
      'Sınırsız kural',
      'Sınırsız rezervasyon',
      'Email + SMS bildirimleri',
      'CRM tier sistemi',
      'Kapora modülü',
      'Gelişmiş raporlar',
    ],
    limits: {
      venues: 1,
      tables: 'unlimited',
      rules: 'unlimited',
      reservationsPerMonth: 'unlimited',
    },
  },
  {
    id: 'business',
    name: 'Business',
    priceMonthly: 1990,
    priceYearly: 19900,
    features: [
      '3 mekan',
      'Sınırsız masa',
      'Sınırsız kural',
      'Sınırsız rezervasyon',
      'Email + SMS',
      'API erişimi',
      'Webhook',
      'Öncelikli destek',
      'Özel entegrasyonlar',
    ],
    limits: {
      venues: 3,
      tables: 'unlimited',
      rules: 'unlimited',
      reservationsPerMonth: 'unlimited',
    },
  },
  {
    id: 'enterprise',
    name: 'Enterprise',
    priceMonthly: null,
    priceYearly: null,
    features: [
      'Sınırsız mekan',
      'White-label',
      'SLA garantisi',
      'Özel geliştirme',
      'Dedike destek',
      'Özel database',
      'SSO',
    ],
    limits: {
      venues: 'unlimited',
      tables: 'unlimited',
      rules: 'unlimited',
      reservationsPerMonth: 'unlimited',
    },
  },
]

export function SubscriptionPage() {
  const [isYearly, setIsYearly] = useState(false)
  
  // TODO: Fetch from auth store/API
  const currentPlan = 'starter'
  const usage = {
    venues: 1,
    tables: 2,
    rules: 3,
    reservationsThisMonth: 47,
  }
  const renewalDate = new Date(2026, 5, 11)
  const invoices = [
    { id: '1', date: new Date(2026, 4, 11), amount: 490, status: 'paid' },
    { id: '2', date: new Date(2026, 3, 11), amount: 490, status: 'paid' },
  ]

  const handleUpgrade = (planId: string) => {
    if (planId === 'enterprise') {
      toast.info('Enterprise plan için satış ekibimizle iletişime geçin')
      return
    }
    console.log('Upgrading to:', planId)
    // TODO: İyzico checkout flow
    toast.success('Ödeme sayfasına yönlendiriliyorsunuz...')
  }

  const handleCancelSubscription = () => {
    if (!confirm('Aboneliğinizi iptal etmek istediğinizden emin misiniz? Mevcut dönem sonunda planınız sona erecektir.')) {
      return
    }
    console.log('Cancel subscription')
    toast.success('Abonelik iptali talebiniz alındı')
  }

  const currentPlanData = plans.find((p) => p.id === currentPlan)

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Abonelik</h1>
        <p className="text-muted-foreground">Plan ve fatura yönetimi</p>
      </div>

      {/* Current Plan */}
      <Card className="border-primary">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-2xl">{currentPlanData?.name} Plan</CardTitle>
              <CardDescription>
                Yenileme tarihi: {format(renewalDate, 'dd MMMM yyyy', { locale: tr })}
              </CardDescription>
            </div>
            <Badge className="text-lg px-4 py-2">
              ₺{isYearly ? currentPlanData?.priceYearly : currentPlanData?.priceMonthly} / {isYearly ? 'yıl' : 'ay'}
            </Badge>
          </div>
        </CardHeader>
        <CardContent className="space-y-6">
          {/* Usage */}
          <div className="space-y-4">
            <h3 className="font-medium">Bu Ay Kullanım</h3>

            <div className="space-y-3">
              <div className="space-y-1">
                <div className="flex items-center justify-between text-sm">
                  <span>Rezervasyonlar</span>
                  <span className="font-medium">
                    {usage.reservationsThisMonth} / {currentPlanData?.limits.reservationsPerMonth === 'unlimited' ? '∞' : currentPlanData?.limits.reservationsPerMonth}
                  </span>
                </div>
                {currentPlanData?.limits.reservationsPerMonth !== 'unlimited' && (
                  <Progress value={(usage.reservationsThisMonth / (currentPlanData?.limits.reservationsPerMonth as number)) * 100} />
                )}
              </div>

              <div className="space-y-1">
                <div className="flex items-center justify-between text-sm">
                  <span>Kurallar</span>
                  <span className="font-medium">
                    {usage.rules} / {currentPlanData?.limits.rules === 'unlimited' ? '∞' : currentPlanData?.limits.rules}
                  </span>
                </div>
                {currentPlanData?.limits.rules !== 'unlimited' && (
                  <Progress value={(usage.rules / (currentPlanData?.limits.rules as number)) * 100} />
                )}
              </div>

              <div className="space-y-1">
                <div className="flex items-center justify-between text-sm">
                  <span>Masalar</span>
                  <span className="font-medium">
                    {usage.tables} / {currentPlanData?.limits.tables === 'unlimited' ? '∞' : currentPlanData?.limits.tables}
                  </span>
                </div>
                {currentPlanData?.limits.tables !== 'unlimited' && (
                  <Progress value={(usage.tables / (currentPlanData?.limits.tables as number)) * 100} />
                )}
              </div>
            </div>
          </div>

          {/* Features */}
          <div className="space-y-2">
            <h3 className="font-medium">Özellikler</h3>
            <div className="grid gap-2">
              {currentPlanData?.features.map((feature, i) => (
                <div key={i} className="flex items-center gap-2 text-sm">
                  <Check className="h-4 w-4 text-green-500" />
                  <span>{feature}</span>
                </div>
              ))}
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Plan Comparison */}
      <div className="space-y-4">
        <div className="flex items-center justify-between">
          <h2 className="text-2xl font-bold">Planlar</h2>
          <div className="flex items-center gap-2">
            <Label htmlFor="billing-toggle">Aylık</Label>
            <Switch
              id="billing-toggle"
              checked={isYearly}
              onCheckedChange={setIsYearly}
            />
            <Label htmlFor="billing-toggle">Yıllık</Label>
            {isYearly && (
              <Badge variant="secondary" className="ml-2">
                2 ay bedava
              </Badge>
            )}
          </div>
        </div>

        <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-4">
          {plans.map((plan) => {
            const isCurrent = plan.id === currentPlan
            const price = isYearly ? plan.priceYearly : plan.priceMonthly

            return (
              <Card key={plan.id} className={isCurrent ? 'border-primary' : ''}>
                <CardHeader>
                  <CardTitle>{plan.name}</CardTitle>
                  <div className="text-3xl font-bold">
                    {price ? `₺${price}` : 'Teklif'}
                    {price && <span className="text-sm font-normal text-muted-foreground">/{isYearly ? 'yıl' : 'ay'}</span>}
                  </div>
                </CardHeader>
                <CardContent className="space-y-4">
                  <div className="space-y-2">
                    {plan.features.map((feature, i) => (
                      <div key={i} className="flex items-start gap-2 text-sm">
                        <Check className="h-4 w-4 text-green-500 flex-shrink-0 mt-0.5" />
                        <span>{feature}</span>
                      </div>
                    ))}
                  </div>

                  {isCurrent ? (
                    <Button variant="outline" className="w-full" disabled>
                      Mevcut Plan
                    </Button>
                  ) : (
                    <Button
                      className="w-full"
                      variant={plan.id === 'pro' ? 'default' : 'outline'}
                      onClick={() => handleUpgrade(plan.id)}
                    >
                      {plan.id === 'enterprise' ? 'Teklif Al' : 'Yükselt'}
                    </Button>
                  )}
                </CardContent>
              </Card>
            )
          })}
        </div>
      </div>

      {/* Invoices */}
      <Card>
        <CardHeader>
          <CardTitle>Fatura Geçmişi</CardTitle>
        </CardHeader>
        <CardContent>
          {invoices.length === 0 ? (
            <p className="text-center text-sm text-muted-foreground py-4">
              Henüz fatura yok
            </p>
          ) : (
            <div className="space-y-2">
              {invoices.map((invoice) => (
                <div key={invoice.id} className="flex items-center justify-between border-b pb-2 last:border-0">
                  <div>
                    <div className="font-medium">₺{invoice.amount}</div>
                    <div className="text-sm text-muted-foreground">
                      {format(invoice.date, 'dd MMMM yyyy', { locale: tr })}
                    </div>
                  </div>
                  <div className="flex items-center gap-2">
                    <Badge variant={invoice.status === 'paid' ? 'default' : 'secondary'}>
                      {invoice.status === 'paid' ? 'Ödendi' : 'Beklemede'}
                    </Badge>
                    <Button variant="ghost" size="icon">
                      <Download className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Cancel Subscription */}
      <Card className="border-destructive">
        <CardHeader>
          <CardTitle className="text-destructive">Tehlikeli Bölge</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="flex items-center justify-between">
            <div>
              <p className="font-medium">Aboneliği İptal Et</p>
              <p className="text-sm text-muted-foreground">
                Mevcut dönem sonunda planınız sona erecektir
              </p>
            </div>
            <Button variant="destructive" onClick={handleCancelSubscription}>
              İptal Et
            </Button>
          </div>
        </CardContent>
      </Card>
    </div>
  )
}
