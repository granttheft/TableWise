# Faz 11 — Landing Page

## Proje Bağlamı

Tablewise, Türkiye'deki premium restoranlar, plaj kulüpleri ve lüks mekanlar için
kural tabanlı rezervasyon yönetim platformudur.

Proje yapısı:
```
D:\Projects\TableWise\
├── src/
│   └── Tablewise.Api/           # Backend — port 5086
├── frontend/
│   ├── admin-panel/             # Port 3000 — React 18 + Vite + TailwindCSS + shadcn/ui
│   ├── booking-ui/              # Port 5174 — Müşteri rezervasyon ekranı
│   ├── super-admin/             # Port 3001 — Platform yönetimi
│   └── landing/                 # Port 4000 — BURASI yapılacak (şu an boş)
```

---

## Adım 1 — Backend: Public Pricing Endpoint

Mevcut `/api/platform/pricing` endpoint'i `[Authorize(AuthenticationSchemes = "Platform")]`
ile korumalı — landing page erişemez.

`src/Tablewise.Api/Controllers/` altına yeni bir controller oluştur:

**`PublicController.cs`**
```csharp
[ApiController]
[AllowAnonymous]
[Route("api/public")]
[Produces("application/json")]
public sealed class PublicController : ControllerBase
{
    private readonly IMediator _mediator;
    public PublicController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Landing page için public fiyatlandırma planlarını döner.
    /// </summary>
    [HttpGet("pricing")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanPricingDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PlanPricingDto>>> GetPublicPricing(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetPricingPlansQuery(), cancellationToken);
        return Ok(result);
    }
}
```

`GetPricingPlansQuery` ve `PlanPricingDto` zaten mevcut — sadece yeni controller gerekiyor.

---

## Adım 2 — Landing Projesi Kurulumu

`frontend/landing/` klasörü mevcut ama boş (sadece README.md var).
Buraya tam bir Vite + React 18 + TailwindCSS projesi kur:

```bash
cd frontend/landing
npm create vite@latest . -- --template react-ts
npm install
npm install -D tailwindcss postcss autoprefixer tailwindcss-animate
npm install lucide-react react-router-dom axios
npx tailwindcss init -p
```

### `vite.config.ts`
```typescript
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: { '@': path.resolve(__dirname, './src') },
  },
  server: {
    port: 4000,
    host: true,
    proxy: {
      '/api': {
        target: 'http://localhost:5086',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
```

### `tailwind.config.ts`

Admin panel ile **aynı renk sistemini** kullan — tutarlılık kritik:

```typescript
export default {
  darkMode: ['class'],
  content: ['./src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        // Landing page özel renk paleti
        'landing-bg':      '#0D1117',   // Ana arka plan
        'landing-surface': '#161B22',   // Card arka planı
        'landing-border':  '#21262D',   // Kenarlıklar
        'landing-gold':    '#C9A96E',   // Altın aksanlar (sadece CTA ve ikonlar)
        'landing-gold-hover': '#D4B483',
        'landing-text':    '#E6EDF3',   // Birincil metin
        'landing-muted':   '#8B949E',   // İkincil metin
        // Admin panel ile ortak
        primary: { DEFAULT: '#0f172a' },
        accent:  { DEFAULT: '#f59e0b' },
      },
      fontFamily: {
        sans:    ['Inter', 'sans-serif'],
        display: ['Inter', 'sans-serif'],
      },
      animation: {
        'fade-in-up': 'fadeInUp 0.6s ease-out forwards',
        'fade-in':    'fadeIn 0.4s ease-out forwards',
      },
      keyframes: {
        fadeInUp: {
          '0%':   { opacity: '0', transform: 'translateY(20px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%':   { opacity: '0' },
          '100%': { opacity: '1' },
        },
      },
    },
  },
  plugins: [require('tailwindcss-animate')],
}
```

### `src/index.css`
```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  * { @apply border-landing-border; }
  body {
    @apply bg-landing-bg text-landing-text;
    font-family: 'Inter', sans-serif;
    -webkit-font-smoothing: antialiased;
  }
  ::-webkit-scrollbar { width: 6px; }
  ::-webkit-scrollbar-track { @apply bg-landing-bg; }
  ::-webkit-scrollbar-thumb { @apply bg-landing-border rounded-full; }
}
```

---

## Adım 3 — Proje Dosya Yapısı

```
frontend/landing/src/
├── components/
│   ├── layout/
│   │   ├── Navbar.tsx
│   │   └── Footer.tsx
│   └── sections/
│       ├── Hero.tsx
│       ├── SocialProof.tsx
│       ├── ProblemSolution.tsx
│       ├── Features.tsx
│       ├── HowItWorks.tsx
│       ├── Pricing.tsx
│       ├── Testimonials.tsx
│       └── CtaBanner.tsx
├── hooks/
│   └── usePricing.ts
├── types/
│   └── pricing.ts
├── App.tsx
└── main.tsx
```

---

## Adım 4 — Tip Tanımları

### `src/types/pricing.ts`
```typescript
export interface PlanPricing {
  id: string
  planName: string           // 'Starter' | 'Pro' | 'Enterprise'
  monthlyPrice: number       // ₺ cinsinden
  yearlyPrice: number        // ₺ cinsinden (aylık eşdeğer)
  currency: string           // 'TRY'
  isActive: boolean
  features?: string[]        // Opsiyonel özellik listesi
}
```

---

## Adım 5 — Pricing Hook

### `src/hooks/usePricing.ts`
```typescript
import { useState, useEffect } from 'react'
import type { PlanPricing } from '@/types/pricing'

export function usePricing() {
  const [plans, setPlans] = useState<PlanPricing[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState(false)

  useEffect(() => {
    fetch('/api/public/pricing')
      .then(r => r.json())
      .then(data => {
        setPlans(data)
        setLoading(false)
      })
      .catch(() => {
        setError(true)
        setLoading(false)
      })
  }, [])

  return { plans, loading, error }
}
```

---

## Adım 6 — Sabitler ve Yönlendirmeler

### `src/config.ts`
```typescript
// Ortama göre URL'leri ayarla
export const ADMIN_PANEL_URL =
  import.meta.env.VITE_ADMIN_PANEL_URL ?? 'http://localhost:3000'

export const BOOKING_UI_URL =
  import.meta.env.VITE_BOOKING_UI_URL ?? 'http://localhost:5174'

// Fallback fiyatlar (API erişilemezse gösterilir)
export const FALLBACK_PLANS = [
  { planName: 'Starter',    monthlyPrice: 1490, yearlyPrice: 1192 },
  { planName: 'Pro',        monthlyPrice: 2990, yearlyPrice: 2392 },
  { planName: 'Enterprise', monthlyPrice: 0,    yearlyPrice: 0    },
]
```

### `.env.example`
```
VITE_ADMIN_PANEL_URL=http://localhost:3000
VITE_BOOKING_UI_URL=http://localhost:5174
```

---

## Adım 7 — Component'lar

### `src/components/layout/Navbar.tsx`

- Koyu arka plan (`bg-landing-bg/95`) + backdrop blur
- Scroll'da border-bottom beliriyor
- Sol: "Tablewise" wordmark (Inter bold, altın nokta detayı)
- Orta: "Özellikler" "Fiyatlandırma" "Hakkımızda" (smooth scroll ile `#features`, `#pricing`, `#about`)
- Sağ: "Giriş Yap" (ghost border buton → `ADMIN_PANEL_URL`) + "Ücretsiz Başla" (altın filled buton → `ADMIN_PANEL_URL/register`)
- Mobile: hamburger menü
- Sticky, `z-50`

### `src/components/sections/Hero.tsx`

Üst kısım:
- Küçük badge: `✦ Premium Mekanlar İçin` (altın border, subtle arka plan)
- H1 (3 satır, serif hissi için `font-bold tracking-tight text-5xl md:text-7xl`):
  ```
  Rezervasyonlarınızı
  Siz Yönetin.
  Kurallarınıza Göre.
  ```
- Subtitle: "Tablewise, premium mekanlar için tasarlanmış akıllı rezervasyon platformudur. Kural motoru, anlık masa takibi ve WhatsApp bildirimleri — hepsi tek ekranda."
- CTA'lar: "14 Gün Ücretsiz Dene" (altın filled, `ADMIN_PANEL_URL/register`) + "Demo İzle →" (text link)
- Trust line: `· Kredi kartı gerekmez · Kurulum 15 dakika · 7/24 destek`

Alt kısım (hero görsel):
- `overflow-hidden rounded-xl border border-landing-border shadow-2xl` wrapper
- İçinde dark mock dashboard: masa timeline grid (T-01'den T-05'e, renkli rezervasyon blokları)
- Bu gerçek bir screenshot DEĞİL, CSS ile yapılmış basit bir illüstrasyon:
  - 5 satır masa (T-01..T-05), her biri farklı renkli `rounded-md` bloklar içeriyor
  - Blok renkleri: altın (#C9A96E), amber, slate (durum renkleri)
  - Üstte zaman çizgisi (18:00..24:00)
  - Sağ altta küçük WhatsApp toast: "WhatsApp gönderildi · Şahin ailesine rezervasyon onayı"
  - Tüm bunlar Tailwind ile çizilmiş, resim kullanılmıyor

### `src/components/sections/SocialProof.tsx`

- Koyu muted strip
- Metin: "İstanbul'un önde gelen mekanları Tablewise'a güveniyor"
- 5 placeholder venue ismi metin olarak (logo placeholder'ları `opacity-40` ile)

### `src/components/sections/ProblemSolution.tsx`

İki kolon grid:
- Sol (Problem): "Telefon, Excel, Kâğıt. Hâlâ mı?"
  - 3 pain point: X ikonu + kırmızı-ish metin
- Sağ (Çözüm): "Tablewise ile kurallarınız otomatik işler."
  - 3 çözüm: checkmark + altın renk
- Arka plan: çok hafif gradient

### `src/components/sections/Features.tsx`

id="features"

6 kart, 3x2 grid, dark card (`bg-landing-surface border border-landing-border`):

| İkon | Başlık | Açıklama |
|------|--------|----------|
| Target | Kural Motoru | Grup kompozisyonu, min. harcama kuralları — siz tanımlayın, sistem uygulasın. |
| CreditCard | Kapora Yönetimi | İyzico altyapısıyla kapora doğrudan hesabınıza geçer. Anında, güvenli. `[İsteğe Bağlı Modül]` badge |
| MessageCircle | WhatsApp Bildirimleri | Onay, hatırlatıcı ve iptal bildirimleri müşterilere otomatik gider. |
| Calendar | Gerçek Zamanlı Masa Takibi | Hangi masa dolu, hangisi boş — saniye saniye görün. |
| Users | Ekip Yönetimi | Personel rolleri, yetki seviyeleri tek panelden. |
| BarChart2 | Raporlama & Analitik | Doluluk, kapora gelirleri, tekrar oranı — veriye dayalı kararlar. |

İkonlar `lucide-react`'tan. Arka plan: `bg-landing-gold/10 p-3 rounded-lg` wrapper.

**Kapora kartına** küçük `[İsteğe Bağlı]` badge ekle (altın renk, küçük font) — kapora core özellik değil.

### `src/components/sections/HowItWorks.tsx`

3 adım, yatay akış, altın numbered circles:

1. **Mekanınızı Tanımlayın** — Masalar, salonlar, kapasiteler. 15 dakika.
2. **Kurallarınızı Girin** — Hangi gruba, hangi masa, hangi şart. Kod yok.
3. **Rezervasyonları Alın** — Müşteriler online rezervasyon yapar, siz onaylarsınız.

Adımlar arası connecting line (`border-t border-dashed border-landing-border`).

### `src/components/sections/Pricing.tsx`

id="pricing"

**Toggle: Aylık / Yıllık** — yıllıkta `%20 indirim` badge

**Dinamik fiyat yükleme** (`usePricing` hook'u kullan):
- Loading: skeleton cards (3 adet, `animate-pulse`)
- Error: fallback fiyatlar (FALLBACK_PLANS sabitinden)
- Success: API'den gelen veriler

Plan mapping (API'den gelen `planName`'e göre):
```typescript
const PLAN_META = {
  Starter: {
    subtitle: 'Tek mekan, temel özellikler',
    highlight: false,
    features: ['1 mekan, 50 masa', 'Rezervasyon yönetimi', 'WhatsApp bildirimleri', 'Email destek'],
    cta: 'Başla',
    ctaUrl: `${ADMIN_PANEL_URL}/register?plan=starter`,
  },
  Pro: {
    subtitle: 'Büyüyen mekanlar için',
    highlight: true,  // gold border, "En Popüler" badge
    features: ['3 mekan, sınırsız masa', 'Kural motoru (tam erişim)', 'Raporlama & analitik', 'Öncelikli destek'],
    cta: '14 Gün Ücretsiz Dene',
    ctaUrl: `${ADMIN_PANEL_URL}/register?plan=pro`,
  },
  Enterprise: {
    subtitle: 'Zincir mekanlar ve gruplar için',
    highlight: false,
    features: ['Sınırsız mekan', 'Özel kural motoru', 'Kendi WhatsApp numaranız', 'Dedicated account manager'],
    cta: 'Bize Yazın',
    ctaUrl: 'mailto:hello@tablewise.com.tr',
  },
}
```

Enterprise kart fiyat yerine "Fiyat teklifi alın" göster.

### `src/components/sections/Testimonials.tsx`

3 quote card, `border-l-2 border-landing-gold` sol çizgi:

```
"Çift rezervasyon sorunumuz tamamen bitti. Artık sadece misafirlerimize odaklanıyoruz."
— Ahmet Y., İstanbul Fine Dining

"Kapora işlemleri için ayrı bir sistem kullanıyorduk. Tablewise hepsini tek çatı altında topladı."
— Selin K., Rooftop Bar Sahibi

"Erkek grubu kısıtlamasını otomatik uygulamak harika. Kapıda tartışma kalmadı."
— Murat D., Beach Club Müdürü
```

### `src/components/sections/CtaBanner.tsx`

Full-width, hafif altın glow arka plan:
- H2: "Rezervasyon kaosunu bitirmeye hazır mısınız?"
- P: "14 gün boyunca tüm özellikleri ücretsiz kullanın. Kredi kartı gerekmez."
- Buton: "Hemen Başla — Ücretsiz" → `ADMIN_PANEL_URL/register`

### `src/components/layout/Footer.tsx`

- 4 kolon grid
- Sol: Logo + "Premium mekanlar için rezervasyon platformu" + © 2026 Tablewise
- Kolonlar:
  - Ürün: Özellikler, Fiyatlandırma, Güvenlik
  - Şirket: Hakkımızda, Blog, İletişim
  - Destek: Dokümantasyon, SSS, Durum
- Sağ alt: "Giriş Yap" + "Kayıt Ol" butonları
- Alt çizgi: Gizlilik Politikası · Kullanım Koşulları

---

## Adım 8 — `App.tsx`

```tsx
import { useEffect } from 'react'
import Navbar from '@/components/layout/Navbar'
import Footer from '@/components/layout/Footer'
import Hero from '@/components/sections/Hero'
import SocialProof from '@/components/sections/SocialProof'
import ProblemSolution from '@/components/sections/ProblemSolution'
import Features from '@/components/sections/Features'
import HowItWorks from '@/components/sections/HowItWorks'
import Pricing from '@/components/sections/Pricing'
import Testimonials from '@/components/sections/Testimonials'
import CtaBanner from '@/components/sections/CtaBanner'

export default function App() {
  return (
    <div className="min-h-screen bg-landing-bg">
      <Navbar />
      <main>
        <Hero />
        <SocialProof />
        <ProblemSolution />
        <Features />
        <HowItWorks />
        <Pricing />
        <Testimonials />
        <CtaBanner />
      </main>
      <Footer />
    </div>
  )
}
```

---

## Adım 9 — Tasarım Kuralları

**Kesinlikle uyulması gerekenler:**

1. **Dark mode only** — landing page light mode desteklemez
2. **Altın rengi sadece şunlarda kullan:** CTA butonları, feature ikonları, liste işaretleri, sol border aksanları
3. **Altın rengi KULLANMA:** büyük arka planlar, heading metinleri, bold body metinleri
4. **Boşluk:** section'lar arası `py-24`, card'lar arası `gap-6`
5. **Animasyon:** `animate-fade-in-up` — sadece section'ların ilk elementine, `animation-delay` ile stagger
6. **Font boyutları:** H1: `text-5xl md:text-7xl`, H2: `text-3xl md:text-4xl`, body: `text-base md:text-lg`
7. **Responsive:** mobile-first, tüm grid'ler `grid-cols-1 md:grid-cols-2 lg:grid-cols-3`
8. **Kapora kartı:** diğer feature'lardan ayrı değil ama `[İsteğe Bağlı]` badge ile işaretli

---

## Adım 10 — Çalıştırma

```bash
cd frontend/landing
npm install
npm run dev
# http://localhost:4000 açılmalı
```

Backend çalışıyor olmalı (port 5086) — aksi halde fiyatlar fallback değerlere düşer.

---

## Tamamlanma Kriterleri

- [ ] `http://localhost:4000` sorunsuz açılıyor
- [ ] Navbar linkleri smooth scroll yapıyor
- [ ] "Giriş Yap" ve "Kayıt Ol" butonları `http://localhost:3000`'e yönlendiriyor
- [ ] Pricing section API'den fiyat çekiyor; backend kapalıyken fallback fiyatlar görünüyor
- [ ] Pricing toggle (aylık/yıllık) çalışıyor
- [ ] Hero'daki dashboard illüstrasyonu render ediliyor
- [ ] Tüm section'lar mobile'da düzgün görünüyor
- [ ] Console'da TypeScript hatası yok
- [ ] `npm run build` başarılı
