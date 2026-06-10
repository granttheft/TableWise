import { ADMIN_PANEL_URL } from '@/config'

export default function Footer() {
  return (
    <footer id="about" className="border-t border-landing-border bg-landing-bg">
      <div className="max-w-6xl mx-auto px-6 py-16">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10 mb-12">
          {/* Brand */}
          <div className="md:col-span-1">
            <div className="flex items-center gap-1 font-black text-xl text-landing-text mb-3">
              Tablewise
              <span className="w-1.5 h-1.5 rounded-full bg-landing-gold mb-3 inline-block" />
            </div>
            <p className="text-sm text-landing-muted leading-relaxed">
              Premium mekanlar için rezervasyon platformu.
            </p>
          </div>

          {/* Ürün */}
          <div>
            <h4 className="text-xs font-semibold text-landing-text uppercase tracking-wider mb-4">Ürün</h4>
            <ul className="space-y-2">
              {['Özellikler', 'Fiyatlandırma', 'Güvenlik'].map(item => (
                <li key={item}>
                  <a href="#" className="text-sm text-landing-muted hover:text-landing-text transition-colors">{item}</a>
                </li>
              ))}
            </ul>
          </div>

          {/* Şirket */}
          <div>
            <h4 className="text-xs font-semibold text-landing-text uppercase tracking-wider mb-4">Şirket</h4>
            <ul className="space-y-2">
              {['Hakkımızda', 'Blog', 'İletişim'].map(item => (
                <li key={item}>
                  <a href="#" className="text-sm text-landing-muted hover:text-landing-text transition-colors">{item}</a>
                </li>
              ))}
            </ul>
          </div>

          {/* Destek */}
          <div>
            <h4 className="text-xs font-semibold text-landing-text uppercase tracking-wider mb-4">Destek</h4>
            <ul className="space-y-2">
              {['Dokümantasyon', 'SSS', 'Durum'].map(item => (
                <li key={item}>
                  <a href="#" className="text-sm text-landing-muted hover:text-landing-text transition-colors">{item}</a>
                </li>
              ))}
            </ul>
          </div>
        </div>

        <div className="border-t border-landing-border pt-8 flex flex-col md:flex-row items-center justify-between gap-4">
          <p className="text-xs text-landing-muted">© 2026 Tablewise. Tüm hakları saklıdır.</p>
          <div className="flex items-center gap-4">
            <a href="#" className="text-xs text-landing-muted hover:text-landing-text transition-colors">Gizlilik Politikası</a>
            <span className="text-landing-border">·</span>
            <a href="#" className="text-xs text-landing-muted hover:text-landing-text transition-colors">Kullanım Koşulları</a>
            <span className="text-landing-border">·</span>
            <a href={`${ADMIN_PANEL_URL}/login`} className="text-xs text-landing-muted hover:text-landing-text transition-colors">Giriş Yap</a>
            <a href={`${ADMIN_PANEL_URL}/register`} className="text-xs text-landing-gold hover:text-landing-gold-hover transition-colors font-medium">Kayıt Ol</a>
          </div>
        </div>
      </div>
    </footer>
  )
}
