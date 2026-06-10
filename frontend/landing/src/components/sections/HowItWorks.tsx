const STEPS = [
  {
    number: '01',
    title: 'Mekanınızı Tanımlayın',
    description: 'Masalar, salonlar, kapasiteler. 15 dakika.',
  },
  {
    number: '02',
    title: 'Kurallarınızı Girin',
    description: 'Hangi gruba, hangi masa, hangi şart. Kod yok.',
  },
  {
    number: '03',
    title: 'Rezervasyonları Alın',
    description: 'Müşteriler online rezervasyon yapar, siz onaylarsınız.',
  },
]

export default function HowItWorks() {
  return (
    <section className="py-24 border-t border-landing-border">
      <div className="max-w-6xl mx-auto px-6">
        <div className="text-center mb-14">
          <p className="text-xs text-landing-gold uppercase tracking-widest mb-3">Nasıl Çalışır?</p>
          <h2 className="text-3xl md:text-4xl font-bold text-landing-text">3 adımda hazır</h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-0">
          {STEPS.map((step, i) => (
            <div key={step.number} className="relative flex flex-col items-center text-center px-8">
              {/* Connecting line */}
              {i < STEPS.length - 1 && (
                <div className="hidden md:block absolute top-8 left-1/2 w-full border-t border-dashed border-landing-border z-0" />
              )}
              {/* Number circle */}
              <div className="relative z-10 w-16 h-16 rounded-full bg-landing-surface border-2 border-landing-gold flex items-center justify-center mb-6">
                <span className="font-black text-landing-gold text-lg">{step.number}</span>
              </div>
              <h3 className="font-semibold text-landing-text mb-2">{step.title}</h3>
              <p className="text-sm text-landing-muted leading-relaxed">{step.description}</p>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
