# Özel Limitler — Prompt 3: Admin Panel

## Bağlam

`GET /api/v1/tenant/me/plan-limits` endpoint'i artık custom limitleri de dönüyor.
`PlanLimitsDto.HasCustomLimits` → true ise tenant'a özel limit uygulanıyor.
Admin Panel'de bu limitler gösterilecek ve uygulanacak.

---

## Adım 1 — Yeni Hook: `usePlanLimits`

**Yeni dosya:** `frontend/admin-panel/src/hooks/usePlanLimits.ts`

```typescript
import { useQuery } from '@tanstack/react-query'
import api from '@/lib/api'

export interface PlanLimits {
  maxVenues: number | null              // null = sınırsız
  currentVenueCount: number
  maxTables: number | null
  currentTableCount: number
  maxRules: number | null
  currentRuleCount: number
  maxReservationsPerMonth: number | null
  currentReservationCount: number
  hasCustomLimits: boolean
}

export function usePlanLimits() {
  return useQuery<PlanLimits>({
    queryKey: ['plan-limits'],
    queryFn: () => api.get('/api/v1/tenant/me/plan-limits').then(r => r.data),
    staleTime: 60_000,
    refetchOnWindowFocus: false,
  })
}

// Yardımcı: limite ulaşıldı mı?
export function isAtLimit(current: number, max: number | null): boolean {
  if (max === null) return false   // sınırsız
  return current >= max
}

// Yardımcı: limite yaklaşıldı mı? (%80+)
export function isNearLimit(current: number, max: number | null): boolean {
  if (max === null) return false
  return current / max >= 0.8 && current < max
}

// Yardımcı: yüzde hesapla
export function limitPercentage(current: number, max: number | null): number {
  if (max === null) return 0
  return Math.min(Math.round((current / max) * 100), 100)
}
```

---

## Adım 2 — Dashboard: Plan Kullanım Widget'ı

**Yeni dosya:** `frontend/admin-panel/src/features/dashboard/components/PlanUsageWidget.tsx`

```tsx
import { usePlanLimits, isAtLimit, isNearLimit, limitPercentage } from '@/hooks/usePlanLimits'
import { Skeleton } from '@/components/ui/skeleton'
import { Building2, Table2, BookOpen, CalendarDays, Sparkles } from 'lucide-react'

interface LimitRowProps {
  icon: React.ReactNode
  label: string
  current: number
  max: number | null
}

function LimitRow({ icon, label, current, max }: LimitRowProps) {
  const atLimit   = isAtLimit(current, max)
  const nearLimit = isNearLimit(current, max)
  const pct       = limitPercentage(current, max)
  const unlimited = max === null

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between text-sm">
        <span className="flex items-center gap-1.5 text-muted-foreground">
          {icon}
          {label}
        </span>
        <span className={
          atLimit   ? 'text-destructive font-medium' :
          nearLimit ? 'text-amber-500 font-medium'   :
                      'text-foreground'
        }>
          {unlimited ? `${current} / ∞` : `${current} / ${max}`}
        </span>
      </div>
      {!unlimited && (
        <div className="h-1.5 w-full rounded-full bg-muted overflow-hidden">
          <div
            className={`h-full rounded-full transition-all ${
              atLimit   ? 'bg-destructive' :
              nearLimit ? 'bg-amber-500'   :
                          'bg-primary'
            }`}
            style={{ width: `${pct}%` }}
          />
        </div>
      )}
      {unlimited && (
        <div className="h-1.5 w-full rounded-full bg-muted" />
      )}
    </div>
  )
}

export function PlanUsageWidget() {
  const { data: limits, isLoading } = usePlanLimits()

  if (isLoading) {
    return (
      <div className="space-y-3">
        {[...Array(4)].map((_, i) => <Skeleton key={i} className="h-8" />)}
      </div>
    )
  }

  if (!limits) return null

  return (
    <div className="space-y-3">
      {limits.hasCustomLimits && (
        <div className="flex items-center gap-1.5 text-xs text-amber-500 pb-1">
          <Sparkles className="h-3 w-3" />
          Özel limitler uygulanıyor
        </div>
      )}

      <LimitRow
        icon={<Building2 className="h-3.5 w-3.5" />}
        label="Mekan"
        current={limits.currentVenueCount}
        max={limits.maxVenues}
      />
      <LimitRow
        icon={<Table2 className="h-3.5 w-3.5" />}
        label="Masa"
        current={limits.currentTableCount}
        max={limits.maxTables}
      />
      <LimitRow
        icon={<BookOpen className="h-3.5 w-3.5" />}
        label="Kural"
        current={limits.currentRuleCount}
        max={limits.maxRules}
      />
      <LimitRow
        icon={<CalendarDays className="h-3.5 w-3.5" />}
        label="Aylık Rezervasyon"
        current={limits.currentReservationCount}
        max={limits.maxReservationsPerMonth}
      />
    </div>
  )
}
```

---

## Adım 3 — Dashboard'a Widget Ekleme

**Dosya:** `frontend/admin-panel/src/features/dashboard/DashboardPage.tsx`

Mevcut dashboard layout'una `PlanUsageWidget`'ı ekle.
Sağ sidebar veya mevcut kart grid'inin altına yerleştir:

```tsx
import { PlanUsageWidget } from './components/PlanUsageWidget'

// Dashboard içinde uygun bir yere:
<Card>
  <CardHeader className="pb-3">
    <CardTitle className="text-sm font-medium">Plan Kullanımı</CardTitle>
  </CardHeader>
  <CardContent>
    <PlanUsageWidget />
  </CardContent>
</Card>
```

---

## Adım 4 — Limit Enforcement: Venue Oluşturma

**Dosya:** Venue oluşturma butonunun olduğu component'i bul.
Muhtemelen: `frontend/admin-panel/src/features/` altında `venues` veya `settings` klasörü.

Venue create butonuna limit kontrolü ekle:

```tsx
import { usePlanLimits, isAtLimit } from '@/hooks/usePlanLimits'

// Component içinde:
const { data: limits } = usePlanLimits()
const venueAtLimit = isAtLimit(
  limits?.currentVenueCount ?? 0,
  limits?.maxVenues ?? null
)

// Butona uygula:
<Button
  disabled={venueAtLimit}
  title={venueAtLimit ? `Mekan limitinize ulaştınız (${limits?.currentVenueCount}/${limits?.maxVenues})` : undefined}
>
  Yeni Mekan Ekle
</Button>
```

---

## Adım 5 — Limit Enforcement: Kural Oluşturma

**Dosya:** Kural oluşturma modalının açıldığı component.
Muhtemelen: `frontend/admin-panel/src/features/rules/` içinde.

```tsx
const { data: limits } = usePlanLimits()
const ruleAtLimit = isAtLimit(
  limits?.currentRuleCount ?? 0,
  limits?.maxRules ?? null
)

<Button
  disabled={ruleAtLimit}
  title={ruleAtLimit ? `Kural limitinize ulaştınız (${limits?.currentRuleCount}/${limits?.maxRules})` : undefined}
>
  Yeni Kural Ekle
</Button>
```

---

## Adım 6 — Limit Uyarı Toast'ları

Venue/kural/masa oluşturulduktan sonra `usePlanLimits`'i invalidate et ve yaklaşma uyarısı göster:

```typescript
// Create işlemi başarılı olduktan sonra:
queryClient.invalidateQueries({ queryKey: ['plan-limits'] })

// Yeni limitleri kontrol et ve uyarı göster:
const freshLimits = await queryClient.fetchQuery({ queryKey: ['plan-limits'] })
if (isNearLimit(freshLimits.currentVenueCount, freshLimits.maxVenues)) {
  toast.warning(`Mekan limitinizin %80'ine yaklaştınız.`)
}
```

---

## Tamamlanma Kriterleri

- [ ] Dashboard'da "Plan Kullanımı" kartı görünüyor
- [ ] 4 limit satırı progress bar ile gösteriliyor
- [ ] Sınırsız limitlerde progress bar gösterilmiyor (`∞` yazıyor)
- [ ] %80+ dolduğunda bar amber/sarı renk alıyor
- [ ] Limit dolduğunda bar kırmızı renk alıyor
- [ ] Custom limit varsa "Özel limitler uygulanıyor" badge'i görünüyor
- [ ] Venue/kural create butonları limit dolunca disabled oluyor
- [ ] Super Admin'den custom limit değiştirilince admin panel yenileme sonrası yeni limiti gösteriyor
- [ ] TypeScript hatası yok
