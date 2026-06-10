const QUOTES = [
  {
    text: 'Çift rezervasyon sorunumuz tamamen bitti. Artık sadece misafirlerimize odaklanıyoruz.',
    author: 'Ahmet Y.',
    role: 'İstanbul Fine Dining',
  },
  {
    text: 'Kapora işlemleri için ayrı bir sistem kullanıyorduk. Tablewise hepsini tek çatı altında topladı.',
    author: 'Selin K.',
    role: 'Rooftop Bar Sahibi',
  },
  {
    text: 'Erkek grubu kısıtlamasını otomatik uygulamak harika. Kapıda tartışma kalmadı.',
    author: 'Murat D.',
    role: 'Beach Club Müdürü',
  },
]

export default function Testimonials() {
  return (
    <section className="py-24 border-t border-landing-border">
      <div className="max-w-6xl mx-auto px-6">
        <div className="text-center mb-14">
          <p className="text-xs text-landing-gold uppercase tracking-widest mb-3">Yorumlar</p>
          <h2 className="text-3xl md:text-4xl font-bold text-landing-text">
            Mekan sahipleri ne diyor?
          </h2>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
          {QUOTES.map(({ text, author, role }) => (
            <div
              key={author}
              className="p-6 rounded-xl border border-landing-border bg-landing-surface border-l-2 border-l-landing-gold"
            >
              <p className="text-sm text-landing-text/80 leading-relaxed mb-6 italic">"{text}"</p>
              <div>
                <p className="text-sm font-semibold text-landing-text">— {author}</p>
                <p className="text-xs text-landing-muted">{role}</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </section>
  )
}
