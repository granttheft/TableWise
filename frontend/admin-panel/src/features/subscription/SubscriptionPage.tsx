import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import { Check } from 'lucide-react'

const plans = [
  {
    name: 'Starter',
    price: '₺490',
    period: '/ay',
    features: [
      '1 mekan',
      '3 masa',
      '5 kural',
      '100 rezervasyon/ay',
      'Email bildirimleri',
    ],
  },
  {
    name: 'Pro',
    price: '₺990',
    period: '/ay',
    features: [
      '1 mekan',
      'Sınırsız masa',
      'Sınırsız kural',
      'Sınırsız rezervasyon',
      'SMS bildirimleri',
      'CRM özellikleri',
      'Kapora modülü',
    ],
    popular: true,
  },
  {
    name: 'Business',
    price: '₺1.990',
    period: '/ay',
    features: [
      '3 mekan',
      'Sınırsız masa',
      'Sınırsız kural',
      'Sınırsız rezervasyon',
      'API erişimi',
      'Öncelikli destek',
      'Özel entegrasyonlar',
    ],
  },
]

export function SubscriptionPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Abonelik</h1>
        <p className="text-muted-foreground">Planınızı yönetin ve yükseltin</p>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        {plans.map((plan) => (
          <Card key={plan.name} className={plan.popular ? 'border-accent' : ''}>
            <CardHeader>
              <div className="flex items-center justify-between">
                <CardTitle>{plan.name}</CardTitle>
                {plan.popular && <Badge variant="default">Popüler</Badge>}
              </div>
              <CardDescription>
                <span className="text-3xl font-bold text-foreground">{plan.price}</span>
                <span className="text-muted-foreground">{plan.period}</span>
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ul className="space-y-2">
                {plan.features.map((feature) => (
                  <li key={feature} className="flex items-center text-sm">
                    <Check className="mr-2 h-4 w-4 text-accent" />
                    {feature}
                  </li>
                ))}
              </ul>
              <Button className="mt-6 w-full" variant={plan.popular ? 'default' : 'outline'}>
                {plan.popular ? 'Yükselt' : 'Planı Seç'}
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  )
}
