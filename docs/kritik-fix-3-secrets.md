# Kritik Fix 3 — Production Secret Yönetimi

## Sorun

`appsettings.json`'da literal değerler var:
- JWT secret: `"YOUR_SUPER_SECRET_KEY_MIN_32_CHARS_REPLACE_IN_PRODUCTION"`
- DB password: `dev_password`
- Redis password: `dev_password`

Faz 9 güvenlik fazına girmeden önce secret yönetimi oturmalı.

---

## Adım 1 — .NET User Secrets (Local Development)

```bash
cd src/Tablewise.Api

# User Secrets başlat (zaten aktifse atla)
dotnet user-secrets init

# Kritik secret'ları taşı
dotnet user-secrets set "JwtSettings:Secret" "BURAYA_MIN_32_KARAKTER_GUCLU_BIR_KEY_YAZ"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5433;Database=tablewise_dev;Username=postgres;Password=BURAYA_GERCEK_SIFRE"
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379,password=BURAYA_REDIS_SIFRESI"
```

User Secrets `%APPDATA%\Microsoft\UserSecrets\` altında saklanır — git'e gitmez.

---

## Adım 2 — appsettings.json'dan Literal Değerleri Temizle

`src/Tablewise.Api/appsettings.json` içinde şu alanları placeholder'a çevir:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "JwtSettings": {
    "Secret": "",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 30,
    "RememberMeRefreshTokenExpirationDays": 90
  },
  "Redis": {
    "ConnectionString": ""
  }
}
```

Boş string bırakma — startup'ta kontrol ekleyeceğiz.

---

## Adım 3 — Startup Validasyonu

`src/Tablewise.Api/Program.cs` içinde `app.Run()` öncesine ekle:

```csharp
// Secret validasyonu — boş secret ile uygulama başlamasın
var jwtSecret = builder.Configuration["JwtSettings:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret) || jwtSecret.Length < 32)
    throw new InvalidOperationException(
        "JwtSettings:Secret tanımlı değil veya 32 karakterden kısa. " +
        "Development için: dotnet user-secrets set \"JwtSettings:Secret\" \"...\"\n" +
        "Production için: JWT_SECRET environment variable set edin.");

var connStr = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connStr))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection tanımlı değil.");
```

---

## Adım 4 — appsettings.Development.json Güncelle

`src/Tablewise.Api/appsettings.Development.json` dosyasına sadece
non-secret development override'ları koy:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:3000",
      "http://localhost:5174",
      "http://localhost:3001",
      "http://localhost:4000"
    ]
  }
}
```

---

## Adım 5 — docker-compose.yml Secret'ları Environment Variable'a Taşı

`docker-compose.yml` içinde literal değerleri environment variable'a çevir:

```yaml
services:
  api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - JwtSettings__Secret=${JWT_SECRET}
      - Redis__ConnectionString=${REDIS_CONNECTION_STRING}

  postgres:
    environment:
      - POSTGRES_PASSWORD=${POSTGRES_PASSWORD}

  redis:
    command: redis-server --requirepass ${REDIS_PASSWORD}
```

---

## Adım 6 — .env.example Güncelle

`.env.example` dosyasını kontrol et, şu alanların olduğunu doğrula:

```bash
# Database
DB_CONNECTION_STRING=Host=localhost;Port=5433;Database=tablewise;Username=postgres;Password=
POSTGRES_PASSWORD=

# JWT
JWT_SECRET=

# Redis
REDIS_CONNECTION_STRING=localhost:6379,password=
REDIS_PASSWORD=
```

---

## Adım 7 — .gitignore Kontrolü

`.gitignore`'da şunların olduğunu doğrula:

```
.env
.env.local
appsettings.Local.json
appsettings.Production.json
```

---

## Adım 8 — Doğrulama

```bash
# User Secrets ile başlamalı — hata vermemeli
cd src/Tablewise.Api && dotnet run

# Secret olmadan başlamamalı — hata vermeli
dotnet user-secrets remove "JwtSettings:Secret"
dotnet run
# → "JwtSettings:Secret tanımlı değil" hatası bekleniyor
# Sonra tekrar ekle:
dotnet user-secrets set "JwtSettings:Secret" "..."
```

## Tamamlanma Kriterleri

- [ ] `appsettings.json`'da literal secret yok
- [ ] `dotnet user-secrets list` → tüm kritik secret'lar görünüyor
- [ ] Startup validasyonu boş secret'ta uygulama başlatmıyor
- [ ] `docker-compose.yml` environment variable kullanıyor
- [ ] `.env.example` güncel
- [ ] `dotnet build` hatasız
