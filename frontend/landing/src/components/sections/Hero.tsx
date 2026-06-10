import { ADMIN_PANEL_URL } from '@/config'

const TABLES = [
  { label: 'T-01', reservations: [{ start: 15, width: 20, color: '#C9A96E', name: 'Yılmaz' }, { start: 50, width: 25, color: '#6E8DC9', name: 'Demir' }] },
  { label: 'T-02', reservations: [{ start: 5,  width: 30, color: '#6E8DC9', name: 'Arslan' }, { start: 60, width: 20, color: '#C9A96E', name: 'Şahin' }] },
  { label: 'T-03', reservations: [{ start: 25, width: 35, color: '#6EC98D', name: 'Kaya'   }] },
  { label: 'T-04', reservations: [{ start: 0,  width: 20, color: '#C96E6E', name: 'Çelik'  }, { start: 40, width: 30, color: '#6EC98D', name: 'Aydın' }] },
  { label: 'T-05', reservations: [{ start: 10, width: 45, color: '#C9A96E', name: 'Koç'    }] },
]

const HOURS = ['18:00', '19:00', '20:00', '21:00', '22:00', '23:00', '00:00']

export default function Hero() {
  return (
    <section className="relative min-h-screen flex flex-col justify-center pt-24 pb-16 overflow-hidden">
      {/* Ambient glow */}
      <div className="absolute top-1/3 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[600px] h-[400px] bg-landing-gold/5 rounded-full blur-3xl pointer-events-none" />

      <div className="relative max-w-6xl mx-auto px-6 w-full">
        {/* Top content */}
        <div className="text-center mb-14">
          <div className="inline-flex items-center gap-2 px-3 py-1.5 rounded-full border border-landing-gold/30 bg-landing-gold/5 text-landing-gold text-xs font-medium mb-8 animate-fade-in">
            <span>✦</span>
            <span>Premium Mekanlar İçin</span>
          </div>

          <h1 className="font-black text-5xl md:text-7xl text-landing-text leading-[1.05] tracking-tight mb-6 animate-fade-in-up" style={{ animationDelay: '0.1s', opacity: 0 }}>
            Rezervasyonlarınızı
            <br />
            <span className="text-landing-muted">Siz Yönetin.</span>
            <br />
            Kurallarınıza Göre.
          </h1>

          <p className="text-base md:text-lg text-landing-muted max-w-2xl mx-auto leading-relaxed mb-10 animate-fade-in-up" style={{ animationDelay: '0.2s', opacity: 0 }}>
            Tablewise, premium mekanlar için tasarlanmış akıllı rezervasyon platformudur.
            Kural motoru, anlık masa takibi ve WhatsApp bildirimleri — hepsi tek ekranda.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-6 animate-fade-in-up" style={{ animationDelay: '0.3s', opacity: 0 }}>
            <a
              href={`${ADMIN_PANEL_URL}/register`}
              className="px-6 py-3 rounded-lg bg-landing-gold hover:bg-landing-gold-hover text-landing-bg font-semibold text-sm transition-colors w-full sm:w-auto text-center"
            >
              14 Gün Ücretsiz Dene
            </a>
            <a
              href="#features"
              onClick={e => { e.preventDefault(); document.getElementById('features')?.scrollIntoView({ behavior: 'smooth' }) }}
              className="text-sm text-landing-muted hover:text-landing-text transition-colors flex items-center gap-1"
            >
              Özellikleri İncele <span>→</span>
            </a>
          </div>

          <p className="text-xs text-landing-muted/60">
            · Kredi kartı gerekmez · Kurulum 15 dakika · 7/24 destek
          </p>
        </div>

        {/* Dashboard illustration — signature element */}
        <div className="overflow-hidden rounded-xl border border-landing-border shadow-2xl bg-landing-surface">
          {/* Mock top bar */}
          <div className="flex items-center justify-between px-4 py-3 border-b border-landing-border bg-landing-bg/60">
            <div className="flex items-center gap-2">
              <div className="w-2.5 h-2.5 rounded-full bg-red-500/70" />
              <div className="w-2.5 h-2.5 rounded-full bg-yellow-500/70" />
              <div className="w-2.5 h-2.5 rounded-full bg-green-500/70" />
            </div>
            <span className="text-xs text-landing-muted font-mono">Masa Takvimi — Bugün</span>
            <span className="text-xs text-landing-gold font-medium">● Canlı</span>
          </div>

          {/* Timeline header */}
          <div className="flex pl-14 pr-4 pt-3 pb-1">
            {HOURS.map(h => (
              <div key={h} className="flex-1 text-[10px] text-landing-muted/60 font-mono">{h}</div>
            ))}
          </div>

          {/* Table rows */}
          <div className="px-4 pb-4 space-y-2">
            {TABLES.map(table => (
              <div key={table.label} className="flex items-center gap-2">
                <span className="w-10 text-[11px] text-landing-muted font-mono shrink-0">{table.label}</span>
                <div className="relative flex-1 h-8 bg-landing-bg rounded-md overflow-hidden">
                  {table.reservations.map((res, i) => (
                    <div
                      key={i}
                      className="absolute top-1 bottom-1 rounded flex items-center px-2"
                      style={{
                        left: `${res.start}%`,
                        width: `${res.width}%`,
                        backgroundColor: res.color + '22',
                        borderLeft: `2px solid ${res.color}`,
                      }}
                    >
                      <span className="text-[9px] font-medium truncate" style={{ color: res.color }}>{res.name}</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>

          {/* WhatsApp toast */}
          <div className="flex justify-end px-4 pb-4">
            <div className="flex items-center gap-2 bg-landing-bg border border-landing-border rounded-lg px-3 py-2 text-xs">
              <span className="text-green-400">●</span>
              <span className="text-landing-muted">WhatsApp gönderildi</span>
              <span className="text-landing-muted/50">·</span>
              <span className="text-landing-text/70">Şahin ailesine rezervasyon onayı</span>
            </div>
          </div>
        </div>
      </div>
    </section>
  )
}
