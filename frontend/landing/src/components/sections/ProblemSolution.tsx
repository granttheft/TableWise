import { X, Check } from 'lucide-react'

const PROBLEMS = [
  "Telefon ile rezervasyon alıyorsunuz, Excel'e not düşüyorsunuz.",
  'Çift rezervasyon olduğunda kapıda tartışma yaşıyorsunuz.',
  'Kural uygulamak için her seferinde manuel kontrol yapıyorsunuz.',
]

const SOLUTIONS = [
  'Müşteriler online rezervasyon yapar, sistem otomatik onaylar.',
  'Gerçek zamanlı masa takibi — çift rezervasyon imkânsız.',
  'Kurallarınızı bir kez tanımlayın, sistem otomatik uygular.',
]

export default function ProblemSolution() {
  return (
    <section className="py-24">
      <div className="max-w-6xl mx-auto px-6">
        <div className="grid grid-cols-1 md:grid-cols-2 gap-8">
          {/* Problem */}
          <div className="p-8 rounded-xl border border-landing-border bg-landing-surface/30">
            <h2 className="text-2xl font-bold text-landing-text mb-2">Telefon, Excel, Kâğıt.</h2>
            <p className="text-landing-muted mb-6 text-sm">Hâlâ mı?</p>
            <ul className="space-y-4">
              {PROBLEMS.map((p, i) => (
                <li key={i} className="flex gap-3">
                  <X size={16} className="text-red-400/70 shrink-0 mt-0.5" />
                  <span className="text-sm text-landing-muted">{p}</span>
                </li>
              ))}
            </ul>
          </div>

          {/* Solution */}
          <div className="p-8 rounded-xl border border-landing-gold/20 bg-landing-gold/5">
            <h2 className="text-2xl font-bold text-landing-text mb-2">Tablewise ile kurallarınız</h2>
            <p className="text-landing-muted mb-6 text-sm">otomatik işler.</p>
            <ul className="space-y-4">
              {SOLUTIONS.map((s, i) => (
                <li key={i} className="flex gap-3">
                  <Check size={16} className="text-landing-gold shrink-0 mt-0.5" />
                  <span className="text-sm text-landing-text/80">{s}</span>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </section>
  )
}
