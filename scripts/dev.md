# Yerel geliştirme (`dev.ps1`)

Tek komutla Postgres/Redis, API, admin panel ve müşteri booking arayüzünü başlatır.

## Hızlı başlatma

Proje kökünden:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\dev.ps1
```

Docker zaten ayaktaysa:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\dev.ps1 -SkipDocker
```

VS Code/Cursor: **Terminal → Run Task → dev: full stack**

## Ne yapar?

1. **Durdur** — Aşağıdaki portlarda dinleyen süreçleri sonlandırır (önceki oturumdan kalmış API/Vite pencereleri):
   - `5086` — Tablewise.Api
   - `3000` — admin-panel (Vite)
   - `5174` — booking-ui (Vite)
2. **Docker** — `docker/docker-compose.yml` ile `postgres` ve `redis` ( `-SkipDocker` ile atlanır)
3. **Başlat** — Üç ayrı terminal penceresi:
   - `dotnet run` → API
   - `npm run dev` → admin-panel
   - `npm run dev` → booking-ui

## URL'ler

| Servis | Adres |
|--------|--------|
| API | http://localhost:5086 |
| Admin panel | http://localhost:3000 |
| Booking UI | http://localhost:5174/rezervasyon/{slug} |

Örnek booking: `http://localhost:5174/rezervasyon/demo-venue` (seed slug’a göre değişir).

## İlk kurulum

```powershell
cd frontend/admin-panel
npm install

cd ../booking-ui
npm install
```

API için `src/Tablewise.Api` altında `dotnet restore` yeterlidir; script `dotnet run` çalıştırır.

## Ortam değişkenleri (booking-ui)

`frontend/booking-ui/.env` (`.env.example` kopyası):

```env
VITE_API_URL=http://localhost:5086
VITE_BOOKING_BASE_URL=http://localhost:5174/rezervasyon
```

## Durdurma

- Her terminal penceresinde **Ctrl+C**
- Veya `dev.ps1`’i tekrar çalıştırın — script önce portları temizler, sonra yeniden başlatır

Postgres/Redis konteynerleri script ile kapatılmaz; `docker compose down` ile durdurun.

## Sorun giderme

| Sorun | Çözüm |
|-------|--------|
| Port zaten kullanımda | Script otomatik kapatır; hâlâ hata varsa Görev Yöneticisi’nden `node` / `dotnet` süreçlerini kontrol edin |
| `node_modules` uyarısı | İlgili `frontend/*` klasöründe `npm install` |
| API bağlanamıyor | Docker postgres/redis ayakta mı: `docker compose -f docker/docker-compose.yml ps` |
| Booking boş / 404 API | `VITE_API_URL` ve CORS; API’nin 5086’da çalıştığını doğrulayın |
