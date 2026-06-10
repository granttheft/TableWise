# Özel Limitler — Prompt 2: Super Admin Panel

## Bağlam

Backend'de `PUT /api/platform/tenants/{id}/custom-limits` endpoint'i artık mevcut.
`TenantDetailPage.tsx` zaten var ve tenant bilgilerini gösteriyor.
Bu prompt'ta `TenantDetailPage.tsx`'e "Özel Limitler" bölümü eklenecek.

---

## Mevcut Dosya

`frontend/super-admin/src/features/tenants/TenantDetailPage.tsx`

Mevcut içeriği koru, sadece özel limit bölümünü ekle.

---

## Eklenecek Tip Tanımları

Dosyanın üstüne ekle:

```typescript
interface CustomLimits {
  maxVenues: number | null           // null = plan limitini kullan
  maxTables: number | null
  maxRules: number | null
  maxReservationsPerMonth: number | null
  maxStaffAccounts: number | null
}

interface PlanLimitsResponse {
  maxVenues: number | null           // null = sınırsız
  currentVenueCount: number
  maxTables: number | null
  currentTableCount: number
  maxRules: number | null
  currentRuleCount: number
  maxReservationsPerMonth: number | null
  currentReservationCount: number
  hasCustomLimits: boolean
}
```

---

## Tenant Detail'e Eklenecek Section

`TenantDetailPage` içinde, mevcut plan/durum kartlarının altına yeni bir kart ekle:

### Veri Çekme

```typescript
// Mevcut tenant sorgusu yanına ekle
const { data: planLimits } = useQuery<PlanLimitsResponse>({
  queryKey: ['tenant-plan-limits', tenantId],
  queryFn: () =>
    api.get(`/api/platform/tenants/${tenantId}/plan-limits`).then(r => r.data),
  enabled: !!tenantId,
  staleTime: 30_000,
})
```

Not: Bu endpoint henüz yoksa oluşturulması gerekir — Super Admin için
`GET /api/platform/tenants/{id}/plan-limits` endpoint'i ekle.
Bu endpoint, ilgili tenant'ın `GetPlanLimitsQuery`'sini o tenant context'inde çalıştırır.

Alternatif: Tenant detay DTO'sundan `customLimitsJson`'ı parse et:

```typescript
// Tenant detay response'unda customLimitsJson varsa:
function parseCustomLimits(json: string | null): CustomLimits {
  if (!json) return {
    maxVenues: null, maxTables: null, maxRules: null,
    maxReservationsPerMonth: null, maxStaffAccounts: null
  }
  try {
    const parsed = JSON.parse(json)
    return {
      maxVenues: parsed.maxVenues ?? null,
      maxTables: parsed.maxTables ?? null,
      maxRules: parsed.maxRules ?? null,
      maxReservationsPerMonth: parsed.maxReservationsPerMonth ?? null,
      maxStaffAccounts: parsed.maxStaffAccounts ?? null,
    }
  } catch { return { maxVenues: null, maxTables: null, maxRules: null, maxReservationsPerMonth: null, maxStaffAccounts: null } }
}
```

### State Yönetimi

```typescript
const [customLimits, setCustomLimits] = useState<CustomLimits>({
  maxVenues: null, maxTables: null, maxRules: null,
  maxReservationsPerMonth: null, maxStaffAccounts: null
})
const [savingLimits, setSavingLimits] = useState(false)

// Tenant yüklenince mevcut custom limitleri doldur
useEffect(() => {
  if (tenant?.customLimitsJson) {
    setCustomLimits(parseCustomLimits(tenant.customLimitsJson))
  }
}, [tenant])
```

### UI: Özel Limitler Kartı

```tsx
<Card>
  <CardHeader>
    <div className="flex items-center justify-between">
      <CardTitle className="text-base flex items-center gap-2">
        <Settings2 className="h-4 w-4" />
        Özel Limitler
      </CardTitle>
      {tenant?.customLimitsJson && (
        <Badge variant="outline" className="text-amber-500 border-amber-500/30">
          Özel limit aktif
        </Badge>
      )}
    </div>
    <p className="text-sm text-muted-foreground">
      Boş bırakılan alanlar için plan limitleri geçerlidir.
      -1 girerek sınırsız yapabilirsiniz.
    </p>
  </CardHeader>
  <CardContent>
    <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
      {[
        { key: 'maxVenues',               label: 'Maks. Mekan',           planVal: planLimits?.maxVenues },
        { key: 'maxTables',               label: 'Maks. Masa',            planVal: planLimits?.maxTables },
        { key: 'maxRules',                label: 'Maks. Kural',           planVal: planLimits?.maxRules },
        { key: 'maxReservationsPerMonth', label: 'Aylık Maks. Rezervasyon', planVal: planLimits?.maxReservationsPerMonth },
        { key: 'maxStaffAccounts',        label: 'Maks. Personel',        planVal: null },
      ].map(({ key, label, planVal }) => (
        <div key={key} className="space-y-1.5">
          <label className="text-xs text-muted-foreground">{label}</label>
          <div className="flex items-center gap-2">
            <Input
              type="number"
              min="-1"
              placeholder={
                planVal === null
                  ? 'Plan: Sınırsız'
                  : planVal === undefined
                  ? 'Plan: —'
                  : `Plan: ${planVal}`
              }
              value={customLimits[key as keyof CustomLimits] ?? ''}
              onChange={e => {
                const val = e.target.value === '' ? null : parseInt(e.target.value)
                setCustomLimits(prev => ({ ...prev, [key]: isNaN(val as number) ? null : val }))
              }}
              className="h-8 text-sm"
            />
            {customLimits[key as keyof CustomLimits] !== null && (
              <button
                onClick={() => setCustomLimits(prev => ({ ...prev, [key]: null }))}
                className="text-muted-foreground hover:text-foreground transition-colors"
                title="Plan limitine dön"
              >
                <X className="h-3.5 w-3.5" />
              </button>
            )}
          </div>
          {customLimits[key as keyof CustomLimits] === -1 && (
            <p className="text-xs text-emerald-500">Sınırsız</p>
          )}
          {customLimits[key as keyof CustomLimits] !== null
            && customLimits[key as keyof CustomLimits] !== -1 && (
            <p className="text-xs text-amber-500">Özel: {customLimits[key as keyof CustomLimits]}</p>
          )}
        </div>
      ))}
    </div>

    <div className="flex items-center gap-3 mt-6 pt-4 border-t">
      <Button
        size="sm"
        disabled={savingLimits}
        onClick={handleSaveLimits}
      >
        {savingLimits
          ? <Loader2 className="mr-2 h-3 w-3 animate-spin" />
          : <Save className="mr-2 h-3 w-3" />}
        Limitleri Kaydet
      </Button>
      <Button
        size="sm"
        variant="ghost"
        disabled={savingLimits}
        onClick={handleResetLimits}
        className="text-muted-foreground"
      >
        Tüm Limitleri Sıfırla (Plan'a Dön)
      </Button>
    </div>
  </CardContent>
</Card>
```

### Save ve Reset Fonksiyonları

```typescript
async function handleSaveLimits() {
  try {
    setSavingLimits(true)
    await api.put(`/api/platform/tenants/${tenantId}/custom-limits`, {
      maxVenues:               customLimits.maxVenues,
      maxTables:               customLimits.maxTables,
      maxRules:                customLimits.maxRules,
      maxReservationsPerMonth: customLimits.maxReservationsPerMonth,
      maxStaffAccounts:        customLimits.maxStaffAccounts,
    })
    toast.success('Özel limitler kaydedildi.')
    queryClient.invalidateQueries({ queryKey: ['tenant-detail', tenantId] })
    queryClient.invalidateQueries({ queryKey: ['tenant-plan-limits', tenantId] })
  } catch {
    toast.error('Limitler kaydedilirken hata oluştu.')
  } finally {
    setSavingLimits(false)
  }
}

async function handleResetLimits() {
  try {
    setSavingLimits(true)
    // Tüm null → backend custom limitleri siler
    await api.put(`/api/platform/tenants/${tenantId}/custom-limits`, {
      maxVenues: null, maxTables: null, maxRules: null,
      maxReservationsPerMonth: null, maxStaffAccounts: null,
    })
    setCustomLimits({
      maxVenues: null, maxTables: null, maxRules: null,
      maxReservationsPerMonth: null, maxStaffAccounts: null,
    })
    toast.success('Tüm özel limitler kaldırıldı. Plan limitleri geçerli.')
    queryClient.invalidateQueries({ queryKey: ['tenant-detail', tenantId] })
  } catch {
    toast.error('Limitler sıfırlanırken hata oluştu.')
  } finally {
    setSavingLimits(false)
  }
}
```

---

## Backend Ek: Super Admin Plan Limits Endpoint

Eğer Super Admin için tenant'ın limit bilgilerini çeken endpoint yoksa `PlatformTenantsController.cs`'e ekle:

```csharp
/// <summary>
/// Belirli bir tenant'ın efektif plan limitlerini döner (custom + plan).
/// </summary>
[HttpGet("{tenantId:guid}/plan-limits")]
[RequirePlatformRole(PlatformRole.SuperAdmin, PlatformRole.Finance)]
[ProducesResponseType(typeof(TenantPlanLimitsDto), StatusCodes.Status200OK)]
public async Task<IActionResult> GetTenantPlanLimits(
    Guid tenantId,
    CancellationToken cancellationToken)
{
    var result = await _mediator.Send(
        new GetTenantPlanLimitsByIdQuery(tenantId), cancellationToken);
    return Ok(result);
}
```

`GetTenantPlanLimitsByIdQuery` → `GetPlanLimitsQueryHandler`'ın tenant-agnostic versiyonu.
TenantId'yi `ITenantService`'den değil, parametre olarak alır.

---

## Tamamlanma Kriterleri

- [ ] Tenant detay sayfasında "Özel Limitler" kartı görünüyor
- [ ] Input placeholder'larında plan limitleri gösteriliyor
- [ ] Değer girilince "Özel: X" label'ı çıkıyor
- [ ] -1 girilince "Sınırsız" label'ı çıkıyor
- [ ] X butonuyla tekil limit plan'a döndürülebiliyor
- [ ] "Tüm Limitleri Sıfırla" butonu tüm custom limitleri kaldırıyor
- [ ] Kaydet sonrası "Özel limit aktif" badge'i görünüyor / kayboluyor
- [ ] TypeScript hatası yok
