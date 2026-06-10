import { Target, CreditCard, MessageCircle, Calendar, Users, BarChart2 } from 'lucide-react'
import type { LucideIcon } from 'lucide-react'

interface Feature {
  icon: LucideIcon
  title: string
  description: string
  badge?: string
}

const FEATURES: Feature[] = [
  {
    icon: Target,
    title: 'Kural Motoru',
    description: 'Grup kompozisyonu, min. harcama kuralları — siz tanımlayın, sistem uygulasın.',
  },
  {
    icon: CreditCard,
    title: 'Kapora Yönetimi',
    description: 'İyzico altyapısıyla kapora doğrudan hesabınıza geçer. Anında, güvenli.',
    badge: 'İsteğe Bağlı',
  },
  {
    icon: MessageCircle,
    title: 'WhatsApp Bildirimleri',
    description: 'Onay, hatırlatıcı ve iptal bildirimleri müşterilere otomatik gider.',
  },
  {
    icon: Calendar,
    title: 'Gerçek Zamanlı Masa Takibi',
    description: 'Hangi masa dolu, hangisi boş — saniye saniye görün.',
  },
  {
    icon: Users,
    title: 'Ekip Yönetimi',
    description: 'Personel rolleri, yetki seviyeleri tek panelden.',
  },
  {
    icon: BarChart2,
    title: 'Raporlama & Analitik',
    description: 'Doluluk, kapora gelirleri, tekrar oranı — veriye dayalı kararlar.',
  },
]

export default function Features() {
  return (
    <section id="features" className="py-24">
      <div className="max-w-6xl mx-auto px-6">
        <div className="text-center mb-14">
          <p className="text-xs text-landing-gold uppercase tracking-widest mb-3">Özellikler</p>
          <h2 className="text-3xl md:text-4xl font-bold text-landing-text">
            Bir mekanın ihtiyacı olan her şey
          </h2>
          <p className="text-landing-muted mt-3 max-w-xl mx-auto text-sm md:text-base">
            Rezervasyon akışından kural motoruna, WhatsApp'tan raporlamaya — hepsi tek platform.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {FEATURES.map(({ icon: Icon, title, description, badge }) => (
            <div
              key={title}
              className="p-6 rounded-xl border border-landing-border bg-landing-surface hover:border-landing-gold/30 transition-colors group"
            >
              <div className="w-10 h-10 rounded-lg bg-landing-gold/10 flex items-center justify-center mb-4 group-hover:bg-landing-gold/15 transition-colors">
                <Icon size={18} className="text-landing-gold" />
              </div>
              <div className="flex items-center gap-2 mb-2">
                <h3 className="font-semibold text-landing-text text-sm">{title}</h3>
                {badge && (
                  <span className="text-[10px] px-1.5 py-0.5 rounded border border-landing-gold/30 text-landing-gold font-medium">
                    {badge}
                  </span>
                )}
              </div>
              <p className="text-sm text-landing-muted leading-relaxed">{description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
