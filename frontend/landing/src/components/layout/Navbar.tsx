import { useState, useEffect } from 'react'
import { Menu, X } from 'lucide-react'
import { ADMIN_PANEL_URL } from '@/config'

export default function Navbar() {
  const [scrolled, setScrolled] = useState(false)
  const [menuOpen, setMenuOpen] = useState(false)

  useEffect(() => {
    const onScroll = () => setScrolled(window.scrollY > 20)
    window.addEventListener('scroll', onScroll)
    return () => window.removeEventListener('scroll', onScroll)
  }, [])

  const scrollTo = (id: string) => {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' })
    setMenuOpen(false)
  }

  return (
    <header
      className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 bg-landing-bg/95 backdrop-blur-sm ${
        scrolled ? 'border-b border-landing-border' : ''
      }`}
    >
      <nav className="max-w-6xl mx-auto px-6 h-16 flex items-center justify-between">
        {/* Wordmark */}
        <a href="/" className="flex items-center gap-1 font-black text-xl text-landing-text tracking-tight">
          Tablewise
          <span className="w-1.5 h-1.5 rounded-full bg-landing-gold mb-3 inline-block" />
        </a>

        {/* Desktop nav */}
        <div className="hidden md:flex items-center gap-8">
          <button onClick={() => scrollTo('features')} className="text-sm text-landing-muted hover:text-landing-text transition-colors">
            Özellikler
          </button>
          <button onClick={() => scrollTo('pricing')} className="text-sm text-landing-muted hover:text-landing-text transition-colors">
            Fiyatlandırma
          </button>
          <button onClick={() => scrollTo('about')} className="text-sm text-landing-muted hover:text-landing-text transition-colors">
            Hakkımızda
          </button>
        </div>

        {/* Desktop CTAs */}
        <div className="hidden md:flex items-center gap-3">
          <a
            href={`${ADMIN_PANEL_URL}/login`}
            className="text-sm text-landing-muted hover:text-landing-text transition-colors px-4 py-2 rounded-lg border border-landing-border hover:border-landing-gold/40"
          >
            Giriş Yap
          </a>
          <a
            href={`${ADMIN_PANEL_URL}/register`}
            className="text-sm font-semibold px-4 py-2 rounded-lg bg-landing-gold hover:bg-landing-gold-hover text-landing-bg transition-colors"
          >
            Ücretsiz Başla
          </a>
        </div>

        {/* Mobile hamburger */}
        <button
          className="md:hidden p-2 text-landing-muted hover:text-landing-text"
          onClick={() => setMenuOpen(v => !v)}
          aria-label="Menü"
        >
          {menuOpen ? <X size={20} /> : <Menu size={20} />}
        </button>
      </nav>

      {/* Mobile menu */}
      {menuOpen && (
        <div className="md:hidden border-t border-landing-border bg-landing-bg px-6 py-4 flex flex-col gap-4">
          <button onClick={() => scrollTo('features')} className="text-sm text-landing-muted text-left">Özellikler</button>
          <button onClick={() => scrollTo('pricing')} className="text-sm text-landing-muted text-left">Fiyatlandırma</button>
          <button onClick={() => scrollTo('about')} className="text-sm text-landing-muted text-left">Hakkımızda</button>
          <div className="flex flex-col gap-2 pt-2 border-t border-landing-border">
            <a href={`${ADMIN_PANEL_URL}/login`} className="text-sm text-center text-landing-muted border border-landing-border rounded-lg py-2">Giriş Yap</a>
            <a href={`${ADMIN_PANEL_URL}/register`} className="text-sm text-center font-semibold bg-landing-gold text-landing-bg rounded-lg py-2">Ücretsiz Başla</a>
          </div>
        </div>
      )}
    </header>
  )
}
