import { useState } from 'react'
import { Check } from 'lucide-react'
import { usePricing, type NormalizedPlan } from '@/hooks/usePricing'
import { ADMIN_PANEL_URL } from '@/config'

const PLAN_META: Record<string, {
  subtitle: string
  highlight: boolean
  features: string[]
  cta: string
  ctaHref: (plan: string) => string
}> = {
  Starter: {
    subtitle: 'Tek mekan, temel özellikler',
    highlight: false,
    features: ['1 mekan, 50 masa', 'Rezervasyon yönetimi', 'WhatsApp bildirimleri', 'Email destek'],
    cta: 'Başla',
    ctaHref: () => `${ADMIN_PANEL_URL}/register?plan=starter`,
  },
  Pro: {
    subtitle: 'Büyüyen mekanlar için',
    highlight: true,
    features: ['3 mekan, sınırsız masa', 'Kural motoru (tam erişim)', 'Raporlama & analitik', 'Öncelikli destek'],
    cta: '14 Gün Ücretsiz Dene',
    ctaHref: () => `${ADMIN_PANEL_URL}/register?plan=pro`,
  },
  Business: {
    subtitle: 'Zincir mekanlar için',
    highlight: false,
    features: ['Sınırsız mekan', 'API & Webhook erişimi', 'Özel entegrasyonlar', 'Dedicated destek'],
    cta: 'Başla',
    ctaHref: () => `${ADMIN_PANEL_URL}/register?plan=business`,
  },
  Enterprise: {
    subtitle: 'Grup ve zincirler için özel çözüm',
    highlight: false,
    features: ['Sınırsız her şey', 'Kendi WhatsApp numaranız', 'White-label seçeneği', 'Dedicated account manager'],
    cta: 'Bize Yazın',
    ctaHref: () => 'mailto:hello@tablewise.com.tr',
  },
}

function SkeletonCard() {
  return (
    <div className="rounded-xl border border-landing-border bg-landing-surface p-6 animate-pulse">
      <div className="h-4 bg-landing-border rounded w-1/2 mb-2" />
      <div className="h-3 bg-landing-border rounded w-3/4 mb-6" />
      <div className="h-10 bg-landing-border rounded w-1/3 mb-6" />
      {[1, 2, 3, 4].map(i => (
        <div key={i} className="h-3 bg-landing-border rounded w-full mb-3" />
      ))}
    </div>
  )
}

function PlanCard({ plan, yearly }: { plan: NormalizedPlan; yearly: boolean }) {
  const meta = PLAN_META[plan.planName] ?? PLAN_META['Starter']
  const price = yearly ? plan.yearlyPrice : plan.monthlyPrice
  const isEnterprise = plan.planName === 'Enterprise'

  return (
    <div
      className={`relative rounded-xl border p-6 flex flex-col ${
        meta.highlight
          ? 'border-landing-gold bg-landing-gold/5'
          : 'border-landing-border bg-landing-surface'
      }`}
    >
      {meta.highlight && (
        <div className="absolute -top-3 left-1/2 -translate-x-1/2 px-3 py-1 bg-landing-gold rounded-full text-landing-bg text-[11px] font-bold">
          En Popüler
        </div>
      )}

      <div className="mb-4">
        <h3 className="font-bold text-landing-text text-lg">{plan.planName}</h3>
        <p className="text-xs text-landing-muted mt-0.5">{meta.subtitle}</p>
      </div>

      <div className="mb-6">
        {isEnterprise ? (
          <span className="text-xl font-bold text-landing-text">Fiyat teklifi alın</span>
        ) : (
          <div className="flex items-end gap-1">
            <span className="text-4xl font-black text-landing-text">
              ₺{price.toLocaleString('tr-TR')}
            </span>
            <span className="text-landing-muted text-sm mb-1">/ay</span>
          </div>
        )}
        {!isEnterprise && yearly && (
          <p className="text-xs text-landing-gold mt-1">Yıllık faturalamayla — %20 indirim</p>
        )}
      </div>

      <ul className="space-y-2.5 flex-1 mb-6">
        {meta.features.map(f => (
          <li key={f} className="flex gap-2.5">
            <Check size={14} className="text-landing-gold shrink-0 mt-0.5" />
            <span className="text-sm text-landing-muted">{f}</span>
          </li>
        ))}
      </ul>

      <a
        href={meta.ctaHref(plan.planName)}
        className={`block text-center text-sm font-semibold py-2.5 rounded-lg transition-colors ${
          meta.highlight
            ? 'bg-landing-gold hover:bg-landing-gold-hover text-landing-bg'
            : 'border border-landing-border hover:border-landing-gold/40 text-landing-text'
        }`}
      >
        {meta.cta}
      </a>
    </div>
  )
}

export default function Pricing() {
  const [yearly, setYearly] = useState(false)
  const { plans, loading } = usePricing()

  return (
    <section id="pricing" className="py-24 border-t border-landing-border">
      <div className="max-w-6xl mx-auto px-6">
        <div className="text-center mb-10">
          <p className="text-xs text-landing-gold uppercase tracking-widest mb-3">Fiyatlandırma</p>
          <h2 className="text-3xl md:text-4xl font-bold text-landing-text mb-4">
            Mekanınıza uygun plan
          </h2>

          {/* Toggle */}
          <div className="inline-flex items-center gap-3 bg-landing-surface border border-landing-border rounded-lg p-1">
            <button
              onClick={() => setYearly(false)}
              className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors ${
                !yearly ? 'bg-landing-bg text-landing-text' : 'text-landing-muted'
              }`}
            >
              Aylık
            </button>
            <button
              onClick={() => setYearly(true)}
              className={`px-4 py-1.5 rounded-md text-sm font-medium transition-colors flex items-center gap-2 ${
                yearly ? 'bg-landing-bg text-landing-text' : 'text-landing-muted'
              }`}
            >
              Yıllık
              <span className="text-[10px] px-1.5 py-0.5 bg-landing-gold/20 text-landing-gold rounded font-bold">
                %20
              </span>
            </button>
          </div>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
          {loading
            ? [1, 2, 3, 4].map(i => <SkeletonCard key={i} />)
            : plans.map(plan => <PlanCard key={plan.id} plan={plan} yearly={yearly} />)
          }
        </div>
      </div>
    </section>
  )
}
