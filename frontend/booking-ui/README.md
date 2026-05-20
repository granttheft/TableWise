# Tablewise Booking UI

Tablewise müşteri rezervasyon arayüzü - React + TypeScript + Vite

## Kurulum

```bash
npm install
```

## Geliştirme

```bash
# .env dosyası oluştur
cp .env.example .env

# Geliştirme sunucusunu başlat
npm run dev
```

Uygulama http://localhost:5174 adresinde çalışacaktır.

## Özellikler

- ✅ 5 adımlı rezervasyon wizard'ı
- ✅ Dinamik custom field desteği
- ✅ Grup kompozisyonu seçici (karma, erkek, kadın, aile)
- ✅ Kural motoru entegrasyonu (BLOCK, WARN, DISCOUNT, DEPOSIT)
- ✅ Rezervasyon yönetimi (görüntüleme, değiştirme, iptal)
- ✅ Responsive tasarım (mobil-first)
- ✅ TypeScript strict mode
- ✅ shadcn/ui komponentleri
- ✅ React Query ile veri yönetimi
- ✅ React Hook Form + Zod validasyonu
- ✅ Framer Motion animasyonları
- ✅ SEO optimizasyonu (React Helmet Async)

## Teknoloji Stack

- React 18
- TypeScript (strict mode)
- Vite
- TailwindCSS
- shadcn/ui
- React Query v5
- React Hook Form + Zod
- React Router Dom v6
- Framer Motion
- Sonner (toasts)
- React Helmet Async

## Klasör Yapısı

```
src/
  ├── components/         # Reusable UI components
  │   ├── ui/            # shadcn/ui base components
  │   ├── ProgressBar.tsx
  │   ├── VenueHeader.tsx
  │   ├── RuleResultBanner.tsx
  │   ├── GroupCompositionPicker.tsx  # Özel grup seçici
  │   ├── CustomFieldRenderer.tsx
  │   └── DepositInfo.tsx
  ├── steps/             # Wizard step components
  │   ├── Step1Date.tsx
  │   ├── Step2TimeAndPeople.tsx
  │   ├── Step3Table.tsx
  │   ├── Step4Info.tsx
  │   └── Step5Confirm.tsx
  ├── pages/             # Route pages
  │   ├── BookingPage.tsx
  │   ├── ConfirmPage.tsx
  │   ├── ViewPage.tsx
  │   ├── CancelPage.tsx
  │   └── ModifyPage.tsx
  ├── hooks/             # Custom React hooks
  │   ├── useVenueConfig.ts
  │   ├── useAvailability.ts
  │   ├── useEvaluate.ts
  │   ├── useReserve.ts
  │   ├── useReservationDetail.ts
  │   └── useDebounce.ts
  ├── lib/               # Utilities
  │   ├── api.ts         # Axios instance + API functions
  │   └── utils.ts       # Helper functions
  └── types/             # TypeScript definitions
      └── api.ts
```

## API Endpoints

- `GET /api/v1/book/{slug}/config` - Venue bilgileri
- `GET /api/v1/book/{slug}/availability` - Müsait slotlar
- `POST /api/v1/book/{slug}/evaluate` - Kural değerlendirmesi
- `POST /api/v1/book/{slug}/reserve` - Rezervasyon oluştur
- `GET /api/v1/book/confirm/{code}` - Rezervasyon detayı
- `PATCH /api/v1/book/confirm/{code}/modify` - Rezervasyon güncelle
- `POST /api/v1/book/confirm/{code}/cancel` - Rezervasyon iptal

## Build

```bash
npm run build
```

Build çıktısı `dist/` klasöründe oluşur.

## Önemli Notlar

1. **Idempotency-Key**: Tüm POST isteklerine otomatik olarak UUID ekler
2. **Group Composition**: Özel component, "group_composition" fieldKey'i ile tetiklenir
3. **Custom Fields**: Dinamik olarak render edilir, tüm field tipleri desteklenir
4. **Rule Evaluation**: 500ms debounce ile otomatik çalışır
5. **Mobil Optimizasyon**: 375px'den başlar, touch-friendly
6. **TypeScript Strict**: Tüm tipler tanımlı, any kullanımı yok

## Lisans

Proprietary - Tablewise © 2026
