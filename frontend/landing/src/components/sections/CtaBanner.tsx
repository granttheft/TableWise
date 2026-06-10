import { ADMIN_PANEL_URL } from '@/config'

export default function CtaBanner() {
  return (
    <section className="py-24 border-t border-landing-border">
      <div className="max-w-3xl mx-auto px-6 text-center">
        {/* Gold glow */}
        <div className="relative">
          <div className="absolute inset-0 bg-landing-gold/10 rounded-2xl blur-2xl" />
          <div className="relative px-8 py-16 rounded-2xl border border-landing-gold/20 bg-landing-surface/60">
            <h2 className="text-3xl md:text-4xl font-bold text-landing-text mb-4">
              Rezervasyon kaosunu bitirmeye
              <br />
              hazır mısınız?
            </h2>
            <p className="text-landing-muted mb-8 text-sm md:text-base max-w-lg mx-auto">
              14 gün boyunca tüm özellikleri ücretsiz kullanın. Kredi kartı gerekmez.
            </p>
            <a
              href={`${ADMIN_PANEL_URL}/register`}
              className="inline-block px-8 py-3.5 rounded-lg bg-landing-gold hover:bg-landing-gold-hover text-landing-bg font-bold text-sm transition-colors"
            >
              Hemen Başla — Ücretsiz
            </a>
          </div>
        </div>
      </div>
    </section>
  )
}
