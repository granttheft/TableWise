# Plan Limitleri — Super Admin'den Dinamik Yönetim

## Amaç

Super Admin panelinden plan limitlerini ve özellik flag'lerini yönetebilmek,
bu verilerin landing page'de dinamik olarak görüntülenmesini sağlamak.

Şu an `Plan` entity'sinde `LimitsJson` ve `FeaturesJson` alanları var ama:
- `PlanPricingDto` bu alanları içermiyor
- Super Admin sadece fiyat güncelleyebiliyor
- Landing page feature listesi hardcoded

## Katman 1 — Backend DTO ve Command Güncellemeleri

### 1.1 `PlanPricingDto` — Güncelle

**Dosya:** `src/Tablewise.Application/DTOs/Platform/CouponDtos.cs`

Mevcut `PlanPricingDto` record'unu şununla değiştir:

```csharp
public record PlanPricingDto(
    Guid Id,
    string Name,
    string Tier,
    decimal MonthlyPriceTry,
    decimal YearlyPriceTry,
    bool IsVisible,
    string LimitsJson,      // YENI: { "maxVenues": 1, "maxTables": 50, "maxRules": 5, "maxReservationsPerMonth": 200 }
    string FeaturesJson);   // YENI: { "depositEnabled": false, "signalREnabled": true, "apiEnabled": false }
```

### 1.2 `UpdatePlanPricingDto` — Güncelle

Aynı dosyada mevcut `UpdatePlanPricingDto`'yu genişlet:

```csharp
public record UpdatePlanPricingDto(
    decimal MonthlyPriceTry,
    decimal YearlyPriceTry,
    string? LimitsJson,     // YENI — null ise mevcut değer korunur
    string? FeaturesJson);  // YENI — null ise mevcut değer korunur
```

### 1.3 `GetPricingPlansQueryHandler` — Güncelle

**Dosya:** `src/Tablewise.Application/Features/Platform/Queries/GetPricingPlansQueryHandler.cs`

Select sorgusuna `LimitsJson` ve `FeaturesJson` ekle:

```csharp
.Select(p => new PlanPricingDto(
    p.Id,
    p.Name,
    p.Tier.ToString(),
    p.MonthlyPriceTry,
    p.YearlyPriceTry,
    p.IsVisible,
    p.LimitsJson,     // YENI
    p.FeaturesJson))  // YENI
```

### 1.4 `UpdatePlanPricingCommand` ve Handler — Güncelle

**Dosya:** `src/Tablewise.Application/Features/Platform/Commands/` altında `UpdatePlanPricingCommand.cs` ve handler'ı bul.

Handler'da güncelleme mantığını genişlet:

```csharp
// Mevcut
plan.MonthlyPriceTry = dto.MonthlyPriceTry;
plan.YearlyPriceTry = dto.YearlyPriceTry;

// EKLE
if (dto.LimitsJson is not null)
{
    // Geçerli JSON mi kontrol et
    try { System.Text.Json.JsonDocument.Parse(dto.LimitsJson); }
    catch { throw new ArgumentException("LimitsJson geçerli bir JSON değil."); }
    plan.LimitsJson = dto.LimitsJson;
}

if (dto.FeaturesJson is not null)
{
    try { System.Text.Json.JsonDocument.Parse(dto.FeaturesJson); }
    catch { throw new ArgumentException("FeaturesJson geçerli bir JSON değil."); }
    plan.FeaturesJson = dto.FeaturesJson;
}
```

---

## Katman 2 — Super Admin Panel: PricingPage Güncellemesi

**Dosya:** `frontend/super-admin/src/features/pricing/PricingPage.tsx`

Mevcut dosyayı tamamen yeniden yaz. Yeni versiyon:

### Tip Tanımları

```typescript
interface PlanLimits {
  maxVenues: number           // Mekan sayısı limiti (-1 = sınırsız)
  maxTables: number           // Masa sayısı limiti (-1 = sınırsız)
  maxRules: number            // Kural sayısı limiti (-1 = sınırsız)
  maxReservationsPerMonth: number  // Aylık rezervasyon limiti (-1 = sınırsız)
  maxStaffAccounts: number    // Personel hesabı limiti (-1 = sınırsız)
}

interface PlanFeatures {
  depositEnabled: boolean     // Kapora modülü (isteğe bağlı eklenti)
  signalREnabled: boolean     // Gerçek zamanlı masa takibi
  apiEnabled: boolean         // API erişimi
  customWhatsAppEnabled: boolean  // Kendi WhatsApp numarası
  analyticsEnabled: boolean   // Gelişmiş analitik
}

interface PlanPricing {
  id: string
  name: string
  tier: string
  monthlyPriceTry: number
  yearlyPriceTry: number
  isVisible: boolean
  limitsJson: string
  featuresJson: string
}
```

### Yardımcı Fonksiyonlar

```typescript
function parseLimits(json: string): PlanLimits {
  try {
    return JSON.parse(json)
  } catch {
    return { maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3 }
  }
}

function parseFeatures(json: string): PlanFeatures {
  try {
    return JSON.parse(json)
  } catch {
    return { depositEnabled: false, signalREnabled: true, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: false }
  }
}

// -1 → "Sınırsız", diğer → sayı
function displayLimit(val: number): string {
  return val === -1 ? 'Sınırsız' : String(val)
}
```

### UI Yapısı

Her plan kartı üç sekmeye ayrılsın (`Tabs` component'i):

**Sekme 1: Fiyatlar** (mevcut fiyat input'ları — aynı kalır)

**Sekme 2: Limitler**
Her limit için bir input satırı:
- Mekan Sayısı (`maxVenues`)
- Masa Sayısı (`maxTables`)
- Kural Sayısı (`maxRules`)
- Aylık Rezervasyon (`maxReservationsPerMonth`)
- Personel Hesabı (`maxStaffAccounts`)

Her input'un yanında küçük checkbox: "Sınırsız" — işaretlenince değer -1 olur ve input disabled olur.

**Sekme 3: Özellikler**
Her feature için bir toggle/switch:
- Kapora Modülü (`depositEnabled`) — tooltip: "İsteğe bağlı eklenti"
- Gerçek Zamanlı Takip (`signalREnabled`)
- API Erişimi (`apiEnabled`)
- Kendi WhatsApp Numarası (`customWhatsAppEnabled`)
- Gelişmiş Analitik (`analyticsEnabled`)

### Kaydet Butonu

Her sekmedeki değişiklik aynı PUT isteğine gönderilsin:

```typescript
await api.put(`/api/platform/pricing/${plan.id}`, {
  monthlyPriceTry: monthly,
  yearlyPriceTry: yearly,
  limitsJson: JSON.stringify(editedLimits),
  featuresJson: JSON.stringify(editedFeatures),
})
```

Dirty state tüm sekmeler için ortak olsun — bir sekmede değişiklik yapılınca "Kaydet" butonu aktif olsun.

### Enterprise Özel Durumu

Enterprise kartı için fiyat input'ları disabled (Özel Teklif) ama limitler ve özellikler düzenlenebilir olsun.

---

## Katman 3 — Landing Page: Pricing Section Güncellemesi

**Dosya:** `frontend/landing/src/hooks/usePricing.ts`

Tip tanımlarını güncelle:

```typescript
export interface PlanPricing {
  id: string
  planName: string
  tier: string
  monthlyPrice: number
  yearlyPrice: number
  currency: string
  isActive: boolean
  limits: {
    maxVenues: number
    maxTables: number
    maxRules: number
    maxReservationsPerMonth: number
    maxStaffAccounts: number
  }
  features: {
    depositEnabled: boolean
    signalREnabled: boolean
    apiEnabled: boolean
    customWhatsAppEnabled: boolean
    analyticsEnabled: boolean
  }
}
```

Hook içinde API response'u parse et:

```typescript
.then(data => {
  const parsed = data.map((p: any) => ({
    id: p.id,
    planName: p.name,
    tier: p.tier,
    monthlyPrice: p.monthlyPriceTry,
    yearlyPrice: p.yearlyPriceTry,
    currency: 'TRY',
    isActive: p.isVisible,
    limits: (() => {
      try { return JSON.parse(p.limitsJson) }
      catch { return { maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3 } }
    })(),
    features: (() => {
      try { return JSON.parse(p.featuresJson) }
      catch { return { depositEnabled: false, signalREnabled: true, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: false } }
    })(),
  }))
  setPlans(parsed)
  setLoading(false)
})
```

**Dosya:** `frontend/landing/src/components/sections/Pricing.tsx`

`PLAN_META` sabitini güncelle — feature listesi artık hardcoded olmayacak, dinamik üretilecek:

```typescript
function buildFeatureList(plan: PlanPricing): string[] {
  const items: string[] = []
  const { limits, features } = plan

  // Limitler
  items.push(
    limits.maxVenues === -1
      ? 'Sınırsız mekan'
      : `${limits.maxVenues} mekan`
  )
  items.push(
    limits.maxTables === -1
      ? 'Sınırsız masa'
      : `${limits.maxTables} masa`
  )
  items.push(
    limits.maxReservationsPerMonth === -1
      ? 'Sınırsız rezervasyon/ay'
      : `${limits.maxReservationsPerMonth} rezervasyon/ay`
  )
  items.push(
    limits.maxStaffAccounts === -1
      ? 'Sınırsız personel hesabı'
      : `${limits.maxStaffAccounts} personel hesabı`
  )

  // Özellikler
  if (features.signalREnabled) items.push('Gerçek zamanlı masa takibi')
  if (features.analyticsEnabled) items.push('Gelişmiş analitik')
  if (features.apiEnabled) items.push('API erişimi')
  if (features.customWhatsAppEnabled) items.push('Kendi WhatsApp numaranız')
  if (features.depositEnabled) items.push('Kapora modülü (isteğe bağlı)')

  return items
}
```

Pricing kartında feature list'i `buildFeatureList(plan)` ile render et:

```tsx
<ul className="space-y-2 mt-4">
  {buildFeatureList(plan).map((feature, i) => (
    <li key={i} className="flex items-center gap-2 text-sm text-landing-muted">
      <Check className="h-3.5 w-3.5 text-landing-gold flex-shrink-0" />
      {feature}
    </li>
  ))}
</ul>
```

---

## Fallback Değerleri

`frontend/landing/src/config.ts` içindeki `FALLBACK_PLANS` sabitini de güncelle:

```typescript
export const FALLBACK_PLANS = [
  {
    planName: 'Starter',
    tier: 'Starter',
    monthlyPrice: 1490,
    yearlyPrice: 1192,
    currency: 'TRY',
    isActive: true,
    limits: { maxVenues: 1, maxTables: 50, maxRules: 5, maxReservationsPerMonth: 200, maxStaffAccounts: 3 },
    features: { depositEnabled: false, signalREnabled: false, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: false },
  },
  {
    planName: 'Pro',
    tier: 'Pro',
    monthlyPrice: 2990,
    yearlyPrice: 2392,
    currency: 'TRY',
    isActive: true,
    limits: { maxVenues: 3, maxTables: -1, maxRules: -1, maxReservationsPerMonth: -1, maxStaffAccounts: 10 },
    features: { depositEnabled: true, signalREnabled: true, apiEnabled: false, customWhatsAppEnabled: false, analyticsEnabled: true },
  },
  {
    planName: 'Enterprise',
    tier: 'Enterprise',
    monthlyPrice: 0,
    yearlyPrice: 0,
    currency: 'TRY',
    isActive: true,
    limits: { maxVenues: -1, maxTables: -1, maxRules: -1, maxReservationsPerMonth: -1, maxStaffAccounts: -1 },
    features: { depositEnabled: true, signalREnabled: true, apiEnabled: true, customWhatsAppEnabled: true, analyticsEnabled: true },
  },
]
```

---

## Tamamlanma Kriterleri

- [ ] Super Admin `PricingPage`'de her plan kartı 3 sekmeye sahip (Fiyatlar / Limitler / Özellikler)
- [ ] Limit input'larında "Sınırsız" checkbox çalışıyor
- [ ] Özellik toggle'ları çalışıyor
- [ ] Kaydet butonu limit + özellik + fiyat değişikliklerini tek PUT ile gönderiyor
- [ ] Landing page feature listesi hardcoded değil, API'den geliyor
- [ ] Super Admin'de plan limitini değiştirince landing page'i yenileyince yeni değer görünüyor
- [ ] Backend kapalıyken landing page fallback değerleri gösteriyor
- [ ] TypeScript hataları yok

## Çalıştırma Sırası

```bash
# 1. Backend
cd src/Tablewise.Api && dotnet run

# 2. Super Admin (limit güncelleme için)
cd frontend/super-admin && npm run dev  # port 3001

# 3. Landing (sonucu görmek için)
cd frontend/landing && npm run dev  # port 4000
```
