# Fable 5 — Tablewise Kapsamlı Proje Değerlendirmesi

## Başlamadan Önce

Sırasıyla şunları oku:
1. `CLAUDE.md` — proje kuralları ve bağlam
2. `graphify-out/GRAPH_REPORT.md` — mimari harita
3. `graphify-out/wiki/index.md` — modül dokümantasyonu
4. `.env.example` — konfigürasyon şablonu
5. `docs/` klasörü — faz dokümanları (varsa)

Sonra aşağıdaki her bölümü **aktif olarak kodu tarayarak** değerlendir.
Tahmin etme — dosyaları aç, oku, somut satır numaraları ver.

---

## BÖLÜM 1 — Mimari Derinlik Analizi

### 1.1 Backend (src/)

Şunları tara ve değerlendir:

**Clean Architecture uyumu:**
- Domain katmanında infrastructure bağımlılığı var mı? (`using Microsoft.EntityFrameworkCore` gibi)
- Application katmanında doğrudan DbContext kullanımı var mı? (IApplicationDbContext dışında)
- Handler'larda N+1 sorgu riski var mı? (`.Include()` zinciri olmadan navigation property erişimi)
- `IgnoreQueryFilters()` kullanılan her yeri listele — meşru mu, risk taşıyor mu?

**CQRS tutarlılığı:**
- Command'lar değer döndürüyor mu? (Anti-pattern)
- Query'ler side effect üretiyor mu?
- MediatR pipeline behavior'ları var mı? (ValidationBehavior, LoggingBehavior vb.)

**Entity kuralları:**
- `TenantId`, `IsDeleted`, `CreatedAt`, `UpdatedAt`, `DeletedAt` — her entity'de var mı?
- Global Query Filter'ı olmayan TenantScoped entity var mı?

**Hata yönetimi:**
- Tüm exception tipleri GlobalExceptionHandler'da ele alınıyor mu?
- Handler'larda `catch {}` boş blok var mı?
- Business exception'lar doğru HTTP koduna map ediliyor mu?

### 1.2 Frontend (frontend/)

**4 frontend için her birini tara:**

Şunları kontrol et:
- TypeScript strict mode aktif mi?
- `any` tipi kullanımı var mı? (Kaç yerde, kritik mi?)
- API response tipleri tam mı? (undefined/null safety)
- React Query error handling tutarlı mı?
- Axios interceptor'lar eksiksiz mi?

**Tip tutarsızlıkları:**
- Aynı kavramı farklı isimle tanımlayan tipler var mı? (`guestName` vs `customerName` gibi)
- Backend DTO'larıyla uyumsuz frontend tipi var mı?
- Enum değerleri backend ile aynı case'de mi?

**State yönetimi:**
- Zustand store'ları gereksiz yere büyümüş mü?
- React Query cache stratejisi optimize mi? (`staleTime`, `gcTime` ayarları)
- Gereksiz re-render'a yol açan pattern var mı?

---

## BÖLÜM 2 — Güvenlik Analizi

Şunları aktif olarak tara:

**Authentication & Authorization:**
- JWT validation parametreleri production için yeterli mi?
- Refresh token rotation tam mı? (Kullanılmış token tekrar kullanılabilir mi?)
- Platform auth ve Tenant auth scheme çakışma noktası var mı?
- `[AllowAnonymous]` olan her endpoint'i listele — meşru mu?

**Input Validation:**
- FluentValidation tüm Command/Query'lerde var mı?
- Eksik validation olan endpoint var mı?
- SQL injection riski — raw query var mı?
- File upload varsa boyut/tip validasyonu var mı?

**Multi-tenant izolasyon:**
- `IgnoreQueryFilters()` kullanılan her yeri tekrar listele
- Cross-tenant erişim mümkün olan senaryo var mı?
- API'de tenant ID URL'den mi, JWT'den mi alınıyor? Tutarlı mı?

**Secrets & Config:**
- `appsettings.json`'da literal secret kalmış mı?
- Environment variable'lar production'da nasıl set ediliyor?
- CORS ayarları production-ready mi?

**Rate limiting:**
- Tüm public endpoint'ler rate limit altında mı?
- Auth endpoint'leri brute force koruması yeterli mi?
- Booking endpoint'leri için özel limit var mı?

---

## BÖLÜM 3 — Performans Analizi

**Backend:**
- N+1 sorgu riski olan handler'ları listele
- Pagination olmayan liste endpoint'i var mı? (Büyük veri seti risken olan)
- Redis cache kullanılması gereken ama kullanılmayan yer var mı?
- Sık çağrılan sorgu için index eksik olabilecek alan var mı?
- Async/await doğru kullanılıyor mu? (`async void`, `Result`, `.Wait()` var mı?)

**Frontend:**
- Bundle size analizi — gereksiz büyük dependency var mı?
- Code splitting uygulanmış mı? (React.lazy, dynamic import)
- İmaj optimizasyonu gerekiyor mu?
- Memoization eksik olan expensive component var mı? (useMemo, useCallback)
- API çağrısı waterfall var mı? (sıralı yerine paralel yapılabilecek)

---

## BÖLÜM 4 — UI/UX Değerlendirmesi

**Her frontend için ayrı değerlendir:**

### Admin Panel
- Rezervasyon listesi için infinite scroll veya virtual list gerekiyor mu?
- Salon/timeline görünümü (varsa) performanslı mı?
- Form validasyon mesajları Türkçe ve anlaşılır mı?
- Klavye navigasyonu çalışıyor mu? (Tab order, Enter, Escape)
- Mobile görünüm var mı? Restoran personeeli tablet/telefon kullanabilir mi?
- Empty state'ler tasarlanmış mı? (Veri yokken ne gösteriyor?)
- Loading skeleton'lar tutarlı mı?
- Hata state'leri kullanıcıya ne yapması gerektiğini söylüyor mu?

### Booking UI
- Rezervasyon akışı kaç adım? Optimize edilebilir mi?
- Progress indicator var mı? (Kullanıcı hangi adımda olduğunu biliyor mu?)
- Form alanları mobile için uygun mu? (Büyük dokunmatik hedef)
- Zaman seçici UX optimize mi?
- Hata mesajları anlaşılır ve yönlendirici mi?
- Confirmtion sayfası yeterli bilgi veriyor mu?

### Super Admin Panel
- Tenant listesi için bulk action var mı? Olmalı mı?
- Limit editörü sezgisel mi? (Sınırsız checkbox pattern doğru mu?)
- Pricing section'da plan karşılaştırması var mı?

### Landing Page
- Hero CTA above the fold mu? (Scroll olmadan görünüyor mu?)
- Pricing toggle (aylık/yıllık) yeterince belirgin mi?
- Social proof güçlü mü? Daha inandırıcı hale getirilebilir mi?
- Form/CTA conversion rate için optimize edilmiş mi?
- Sayfa yüklenme hızı (Core Web Vitals için kritik)
- SEO: meta tags, schema markup, OG tags var mı?
- Mobile görünüm tüm section'larda tam mı?

---

## BÖLÜM 5 — Teknik Borç Envanteri

Şunları tara ve kategorize et:

**Yüksek öncelikli borç:**
- TODO/FIXME yorumları — her birini listele, kritik mi değil mi?
- Hardcoded değerler (URL, port, magic number, string)
- Kullanılmayan import, dead code
- Deprecated API kullanımı

**Orta öncelikli borç:**
- Test edilmemiş kritik business logic
- Duplicate kod (DRY ihlalleri)
- Tutarsız naming convention
- Yorum satırlarıyla devre dışı bırakılmış kod

**Düşük öncelikli borç:**
- Kozmetik tutarsızlıklar
- Aşırı yorumlanmış/az yorumlanmış alanlar

---

## BÖLÜM 6 — Eksik Özellikler Analizi

Mevcut kodu ve CLAUDE.md'deki faz planını karşılaştır.
Şunları değerlendir:

**Beklenip bulunamayanlar:**
- Faz planında "tamamlandı" denen ama eksik kalan parçalar
- Entity var ama UI/API endpoint olmayan özellikler
- Konfigürasyonu olan ama aktif edilmemiş özellikler

**Verimlilik fırsatları:**
- Restoran sahibinin günlük kullanımını zorlaştıran akış var mı?
- Manuel yapılan ama otomatikleştirilebilecek işlem var mı?
- Müşteri (rezervasyon yapan) deneyimini olumsuz etkileyen akış var mı?

---

## BÖLÜM 7 — Deployment Hazırlığı

**Faz 10 öncesi değerlendirme:**

- `Dockerfile` multi-stage build doğru mu?
- `docker-compose.yml` production için uygun mu? (Secrets, restart policy, resource limits)
- Health check endpoint tam mı?
- Nginx konfigürasyonu var mı?
- Database migration production'da nasıl çalışacak? (Otomatik mi, manuel mi?)
- Rollback stratejisi var mı?
- Backup stratejisi var mı?
- Monitoring/alerting kurulu mu?
- Log aggregation hazır mı?

---

## BÖLÜM 8 — İyzico Entegrasyonu Hazırlığı

**Faz 7 öncesi risk değerlendirmesi:**

- `Tenant.IyzicoMerchantId` eklendi mi? (Kritik Fix 5 sonrası)
- `Payment` ve `Refund` entity'leri tam mı?
- Webhook altyapısı hazır mı?
- Alt üye işyeri onay akışı için UI gerekiyor mu?
- Kapora lifecycle (NotRequired → Pending → Paid → Refunded) mantığı eksiksiz mi?
- Para birimi ve kuruş hassasiyeti doğru ele alınıyor mu?

---

## ÇIKTI FORMAT

Bulguları `tablewise-fable-review.md` dosyasına şu yapıda kaydet:

```markdown
# Tablewise — Fable 5 Kapsamlı Değerlendirme
Tarih: [tarih]
Commit: [son commit hash]
Toplam bulgu: [sayı]

## YÖNETİCİ ÖZETİ
[3-5 cümle — en kritik 3 bulgu ve genel sağlık durumu]

## KRİTİK BULGULAR 🔴
[Her bulgu için:]
### [Başlık]
**Dosya:** `path/to/file.cs:satır`
**Sorun:** [Açıklama]
**Risk:** [Ne olabilir?]
**Çözüm Promptu:**
[Doğrudan Claude Code'a yapıştırılabilecek prompt]

## DİKKAT GEREKTİREN BULGULAR ⚠️
[Aynı format]

## GELİŞTİRME FIRSATLARI ✨
[Aynı format — zorunlu değil ama değer katacak şeyler]

## UI/UX ÖNERİLERİ 🎨
[Her frontend için ayrı bölüm]
### [Frontend Adı]
- [Somut öneri + öncelik: Yüksek/Orta/Düşük]
[Öneri için prompt (gerekiyorsa)]

## DEPLOYMENT KONTROLÜ 📦
[Checklist formatında, her madde ✅/⚠️/❌]

## SONRAKI ADIMLAR
[Öncelik sırasına göre 5-10 madde]
[Her madde için tahmini süre]
```

---

## ÖNEMLİ NOTLAR

- Her bulgu için somut dosya yolu ve satır numarası ver
- Tahmin etme — dosyayı aç ve doğrula
- "Muhtemelen", "olabilir" gibi belirsiz ifadelerden kaçın
- Çözüm promptları doğrudan Claude Code'a yapıştırılabilecek kadar detaylı olsun
- UI önerilerinde önce kullanıcı deneyimini düşün, estetik ikincil
- Öncelik sıralaması iş etkisine göre olsun — teknik mükemmellik ikincil
