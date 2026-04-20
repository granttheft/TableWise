# TableWise

> Kural Tabanlı SaaS Rezervasyon ve Masa Yönetim Platformu

TableWise, premium restoranlar, lounge'lar, beach club'lar, pub'lar ve etkinlik mekanları için geliştirilmiş, kural tabanlı rezervasyon ve masa yönetim platformudur.

## 🏗️ Proje Yapısı

```
Tablewise/
├── src/
│   ├── Tablewise.Api/              # ASP.NET Core Web API
│   ├── Tablewise.Application/      # CQRS, Use Cases, DTO'lar
│   ├── Tablewise.Domain/           # Domain Entity ve Value Object'ler
│   ├── Tablewise.Infrastructure/   # EF Core, Redis, External Services
│   └── Tablewise.RuleEngine/       # Custom Kural Motoru Pipeline
├── tests/
│   ├── Tablewise.UnitTests/        # Unit Test'ler
│   └── Tablewise.IntegrationTests/ # Integration Test'ler
├── frontend/
│   ├── admin-panel/                # Admin Panel (app.tablewise.com.tr)
│   ├── booking-ui/                 # Rezervasyon UI (/rezervasyon/[slug])
│   └── landing/                    # Landing Page (tablewise.com.tr)
├── docker/                         # Docker Compose ve Nginx config
├── scripts/                        # Deployment ve utility script'ler
└── docs/                           # Dokümantasyon
```

## 🚀 Teknoloji Stack

### Backend
- **.NET 8** - Web API
- **Clean Architecture** - Katmanlı mimari
- **CQRS** - MediatR ile command/query ayrımı
- **PostgreSQL + EF Core 8** - Veritabanı
- **Redis** - Caching ve idempotency
- **SignalR** - Real-time updates (Pro+ plan)
- **JWT** - Authentication & Authorization
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Sentry** - Error tracking

### Frontend
- **React 18 + Vite** - Modern UI framework
- **TailwindCSS + shadcn/ui** - Styling
- **Zustand + React Query v5** - State management
- **React Hook Form + Zod** - Form handling

### Infrastructure
- **Docker Compose** - Container orchestration
- **Nginx** - Reverse proxy
- **Cloudflare R2** - File storage (S3-compatible)
- **SendGrid** - Email service
- **Netgsm** - SMS service (Pro+ plan)
- **İyzico** - Payment gateway

### External Services
- **SendGrid** - Email notifications
- **Netgsm** - SMS notifications (Pro+ plan)
- **İyzico** - Subscription & deposit payments
- **Cloudflare R2** - File storage

## 🔧 Geliştirme Ortamı Kurulumu

### Gereksinimler
- .NET 8 SDK
- PostgreSQL 15+
- Redis 7+
- Node.js 20+ (Frontend için)
- Docker & Docker Compose (opsiyonel)

### Kurulum

1. **Repository'i klonlayın**
   ```bash
   git clone https://github.com/yourusername/tablewise.git
   cd tablewise
   ```

2. **Solution'ı restore edin**
   ```bash
   dotnet restore
   ```

3. **Gerekli paketleri yükleyin**
   ```bash
   dotnet build
   ```

4. **Environment değişkenlerini ayarlayın**
   ```bash
   cp .env.example .env
   # .env dosyasını düzenleyin
   ```

5. **Veritabanını oluşturun**
   ```bash
   dotnet ef database update --project src/Tablewise.Infrastructure --startup-project src/Tablewise.Api
   ```

6. **Uygulamayı çalıştırın**
   ```bash
   dotnet run --project src/Tablewise.Api
   ```

## 🏛️ Mimari Kararlar

### Clean Architecture
Proje Clean Architecture prensiplerine göre katmanlandırılmıştır:
- **Domain**: İş kuralları ve domain entity'ler
- **Application**: Use case'ler, CQRS handler'lar
- **Infrastructure**: Harici bağımlılıklar (DB, cache, external services)
- **Api**: HTTP endpoints ve middleware'ler

### Multi-Tenancy
- **Tenant → Venue → Table** hiyerarşisi
- Her tabloda `TenantId` (UUID) zorunlu ve indexed
- EF Core Global Query Filter ile otomatik tenant izolasyonu
- İlerde Enterprise plan için DB-per-tenant desteği

### Soft Delete
- Tüm entity'lerde `IsDeleted` ve `DeletedAt` alanları standart
- Global Query Filter ile soft-deleted kayıtlar otomatik filtrelenir

### Kural Motoru
- **Custom IRuleEvaluator Pipeline** (NRules kullanılmıyor)
- JSON-based condition/action definitions
- Version field ile şema migration desteği

### Güvenlik
- JWT + Refresh Token
- BCrypt ile password hashing
- PII loglanmaz (Serilog destructuring policy)
- İyzico webhook signature doğrulama
- Idempotency-Key header zorunlu (POST /reserve)

## 📋 API Versioning
Tüm endpoint'ler `/api/v1/` prefix'i ile başlar.

## 🧪 Test'ler

### Unit Test'ler
```bash
dotnet test tests/Tablewise.UnitTests
```

### Integration Test'ler
```bash
dotnet test tests/Tablewise.IntegrationTests
```

## 📦 Deployment

### Docker ile
```bash
cd docker
docker-compose -f docker-compose.prod.yml up -d
```

### Manuel
1. `dotnet publish -c Release`
2. Nginx konfigürasyonunu ayarlayın
3. SSL sertifikalarını Certbot ile oluşturun
4. Systemd service olarak çalıştırın

## 📝 Lisans
Bu proje özel bir projedir ve tüm hakları saklıdır.

## 📞 İletişim
Destek için: support@tablewise.com.tr

---

**Tablewise** - Premium Mekanlar için Akıllı Rezervasyon Sistemi
