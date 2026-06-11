# Kritik Fix 2 — Health Check Endpoint

## Sorun

`/health` endpoint'i yok. Docker HEALTHCHECK, load balancer ve
Faz 10 deployment için zorunlu. NuGet paketleri yüklü ama
Program.cs'de map edilmemiş.

---

## Adım 1 — NuGet Paketleri Kontrol

`src/Tablewise.Infrastructure/Tablewise.Infrastructure.csproj` veya
`src/Tablewise.Api/Tablewise.Api.csproj` içinde şunların olduğunu doğrula:

```xml
<PackageReference Include="AspNetCore.HealthChecks.NpgSql" Version="8.*" />
<PackageReference Include="AspNetCore.HealthChecks.Redis" Version="8.*" />
```

Yoksa terminal'de ekle:
```bash
cd src/Tablewise.Api
dotnet add package AspNetCore.HealthChecks.NpgSql
dotnet add package AspNetCore.HealthChecks.Redis
dotnet add package AspNetCore.HealthChecks.UI.Client
```

---

## Adım 2 — Program.cs'e Health Check Ekle

`src/Tablewise.Api/Program.cs` içinde `builder.Services` bölümüne ekle:

```csharp
// Health Checks
builder.Services.AddHealthChecks()
    .AddNpgSql(
        builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "postgresql",
        tags: ["db", "ready"])
    .AddRedis(
        builder.Configuration["Redis:ConnectionString"]
            ?? "localhost:6379",
        name: "redis",
        tags: ["cache", "ready"])
    .AddCheck("api", () =>
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API çalışıyor"),
        tags: ["api", "live"]);
```

`app.MapControllers()` satırının hemen altına ekle:

```csharp
// Liveness — sadece API ayakta mı?
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

// Readiness — DB + Redis hazır mı?
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();

// Genel
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
}).AllowAnonymous();
```

using ekle (dosyanın üstüne):
```csharp
using HealthChecks.UI.Client;
```

---

## Adım 3 — Dockerfile'a HEALTHCHECK Ekle

`Dockerfile` içinde `ENTRYPOINT` satırından önce ekle:

```dockerfile
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:5000/health/live || exit 1
```

---

## Adım 4 — docker-compose.yml Güncelle

`docker-compose.yml` içinde api servisine ekle:

```yaml
services:
  api:
    # ... mevcut config
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health/live"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 40s
    depends_on:
      postgres:
        condition: service_healthy
      redis:
        condition: service_healthy
```

PostgreSQL ve Redis servislerine de healthcheck ekle:

```yaml
  postgres:
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 10s
      timeout: 5s
      retries: 5
```

---

## Adım 5 — Doğrulama

```bash
cd src/Tablewise.Api && dotnet run
```

Şunlara istek at:
```
GET http://localhost:5086/health        → 200 OK (JSON detay)
GET http://localhost:5086/health/live   → 200 OK
GET http://localhost:5086/health/ready  → 200 OK (DB + Redis bağlı)
```

## Tamamlanma Kriterleri

- [ ] `/health` → 200 OK JSON döndürüyor
- [ ] `/health/live` → API liveness kontrolü
- [ ] `/health/ready` → DB + Redis hazırlık kontrolü
- [ ] Dockerfile'da HEALTHCHECK direktifi var
- [ ] docker-compose.yml'de depends_on condition güncellendi
- [ ] `dotnet build` hatasız
