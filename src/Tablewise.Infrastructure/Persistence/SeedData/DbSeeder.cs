using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;

namespace Tablewise.Infrastructure.Persistence.SeedData;

/// <summary>
/// Development ortamında demo verileri oluşturur. İdempotent çalışır.
/// </summary>
public class DbSeeder
{
    private readonly TablewiseDbContext _context;
    private readonly ILogger<DbSeeder> _logger;

    /// <summary>
    /// DbSeeder constructor.
    /// </summary>
    /// <param name="context">TablewiseDbContext instance</param>
    /// <param name="logger">Logger instance</param>
    public DbSeeder(TablewiseDbContext context, ILogger<DbSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Seed işlemini çalıştırır. Sadece Development ortamında çalışmalı.
    /// </summary>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Seed data işlemi başlıyor...");

            // İdempotency kontrolü: Demo tenant varsa seed'i atla
            var demoTenantExists = await _context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == SeedIds.DemoTenantId);

            if (demoTenantExists)
            {
                _logger.LogInformation("Demo veriler zaten mevcut, seed atlanıyor.");
                return;
            }

            // Seed sırası önemli (foreign key constraint)
            await SeedPlansAsync();
            await SeedSuperAdminAsync();
            await SeedDemoTenantAsync();
            await SeedDemoVenueAsync();
            await SeedTablesAsync();
            await SeedTableCombinationsAsync();
            await SeedVenueClosuresAsync();
            await SeedVenueCustomFieldsAsync();
            await SeedRulesAsync();
            await SeedCustomersAsync();
            await SeedReservationsAsync();

            await _context.SaveChangesAsync();

            _logger.LogInformation("Seed data işlemi başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Seed data işlemi sırasında hata oluştu.");
            throw;
        }
    }

    private async Task SeedPlansAsync()
    {
        var plans = new List<Plan>
        {
            new()
            {
                Id = SeedIds.PlanStarterId,
                Tier = PlanTier.Starter,
                Name = "Starter",
                Description = "Küçük işletmeler için başlangıç paketi",
                MonthlyPriceTry = 490m,
                YearlyPriceTry = 4900m,
                IsVisible = true,
                FeaturesJson = """
                {
                    "maxVenues": 1,
                    "maxTables": 3,
                    "maxRules": 5,
                    "maxReservationsPerMonth": 100,
                    "enableSms": false,
                    "enableDepositModule": false,
                    "enableApi": false,
                    "enableWhiteLabel": false,
                    "supportLevel": "email"
                }
                """,
                LimitsJson = """
                {
                    "apiRateLimit": 0,
                    "storageQuotaMb": 100
                }
                """,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedIds.PlanProId,
                Tier = PlanTier.Pro,
                Name = "Pro",
                Description = "Büyüyen işletmeler için profesyonel özellikler",
                MonthlyPriceTry = 990m,
                YearlyPriceTry = 9900m,
                IsVisible = true,
                FeaturesJson = """
                {
                    "maxVenues": 1,
                    "maxTables": -1,
                    "maxRules": -1,
                    "maxReservationsPerMonth": -1,
                    "enableSms": true,
                    "enableDepositModule": true,
                    "enableApi": false,
                    "enableWhiteLabel": false,
                    "supportLevel": "priority"
                }
                """,
                LimitsJson = """
                {
                    "apiRateLimit": 0,
                    "storageQuotaMb": 500
                }
                """,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedIds.PlanBusinessId,
                Tier = PlanTier.Business,
                Name = "Business",
                Description = "Çok şubeli işletmeler için kurumsal çözüm",
                MonthlyPriceTry = 1990m,
                YearlyPriceTry = 19900m,
                IsVisible = true,
                FeaturesJson = """
                {
                    "maxVenues": 3,
                    "maxTables": -1,
                    "maxRules": -1,
                    "maxReservationsPerMonth": -1,
                    "enableSms": true,
                    "enableDepositModule": true,
                    "enableApi": true,
                    "enableWhiteLabel": false,
                    "supportLevel": "priority"
                }
                """,
                LimitsJson = """
                {
                    "apiRateLimit": 1000,
                    "storageQuotaMb": 2000
                }
                """,
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = SeedIds.PlanEnterpriseId,
                Tier = PlanTier.Enterprise,
                Name = "Enterprise",
                Description = "Sınırsız özellikler ve özel SLA",
                MonthlyPriceTry = 0m, // Teklif bazlı
                YearlyPriceTry = 0m,
                IsVisible = true,
                FeaturesJson = """
                {
                    "maxVenues": -1,
                    "maxTables": -1,
                    "maxRules": -1,
                    "maxReservationsPerMonth": -1,
                    "enableSms": true,
                    "enableDepositModule": true,
                    "enableApi": true,
                    "enableWhiteLabel": true,
                    "supportLevel": "dedicated"
                }
                """,
                LimitsJson = """
                {
                    "apiRateLimit": -1,
                    "storageQuotaMb": -1
                }
                """,
                CreatedAt = DateTime.UtcNow
            }
        };

        await _context.Plans.AddRangeAsync(plans);
        _logger.LogInformation("4 plan kaydı eklendi.");
    }

    private async Task SeedSuperAdminAsync()
    {
        // SuperAdmin için geçici bir system tenant oluştur (TenantId zorunlu olduğu için)
        // Gerçek implementasyonda SuperAdmin TenantId kontrolünden muaf tutulmalı
        var systemTenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        
        // Şifre: SuperAdmin123! (BCrypt hash)
        var superAdmin = new User
        {
            Id = SeedIds.SuperAdminUserId,
            TenantId = systemTenantId, // Sistem tenant ID'si
            FirstName = "Super",
            LastName = "Admin",
            Email = "superadmin@tablewise.com.tr",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!"),
            PhoneNumber = "+905551234567",
            Role = UserRole.SuperAdmin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Users.AddAsync(superAdmin);
        _logger.LogInformation("SuperAdmin kullanıcısı eklendi.");
    }

    private async Task SeedDemoTenantAsync()
    {
        var demoTenant = new Tenant
        {
            Id = SeedIds.DemoTenantId,
            Name = "Demo Restoran A.Ş.",
            Slug = "demo-restoran",
            Email = "info@demo-restoran.com",
            PasswordHash = string.Empty, // Tenant login yok, sadece User login var
            PlanId = SeedIds.PlanProId,
            PlanStatus = PlanStatus.Active,
            TrialEndsAt = DateTime.UtcNow.AddDays(-15), // Trial 15 gün önce bitmiş
            PlanRenewsAt = DateTime.UtcNow.AddMonths(11), // 12 ay sonra yenilenecek
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        // Demo subscription (Pro plan, aktif dönem)
        var subscription = new Subscription
        {
            Id = SeedIds.DemoSubscriptionId,
            TenantId = SeedIds.DemoTenantId,
            PlanId = SeedIds.PlanProId,
            Status = PlanStatus.Active,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow.AddMonths(11), // 12 aylık aktif
            Amount = 9900m, // Yıllık ödeme
            Currency = "TRY",
            NextBillingDate = DateTime.UtcNow.AddMonths(11),
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        // Demo tenant owner user
        // Şifre: Demo123!
        var demoOwner = new User
        {
            Id = SeedIds.DemoOwnerUserId,
            TenantId = SeedIds.DemoTenantId,
            FirstName = "Ahmet",
            LastName = "Yılmaz",
            Email = "ahmet@demo-restoran.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Demo123!"),
            PhoneNumber = "+905551112234",
            Role = UserRole.Owner,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await _context.Tenants.AddAsync(demoTenant);
        await _context.Subscriptions.AddAsync(subscription);
        await _context.Users.AddAsync(demoOwner);
        _logger.LogInformation("Demo tenant, subscription ve owner eklendi.");
    }

    private async Task SeedDemoVenueAsync()
    {
        var venue = new Venue
        {
            Id = SeedIds.DemoVenueId,
            TenantId = SeedIds.DemoTenantId,
            Name = "Ana Salon",
            Slug = "ana-salon",
            Description = "Bağdat Caddesi'ndeki ana şubemiz",
            Address = "Bağdat Caddesi No:123, Kadıköy, İstanbul",
            PhoneNumber = "+905551112233",
            Email = "anapalon@demo-restoran.com",
            TimeZone = "Europe/Istanbul",
            DefaultOpenTime = new TimeSpan(12, 0, 0), // 12:00
            DefaultCloseTime = new TimeSpan(23, 0, 0), // 23:00
            DefaultSlotDurationMinutes = 90,
            DefaultTurnoverMinutes = 30,
            MaxAdvanceBookingDays = 30,
            MinAdvanceBookingHours = 2,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await _context.Venues.AddAsync(venue);
        _logger.LogInformation("Demo venue eklendi.");
    }

    private async Task SeedTablesAsync()
    {
        var tables = new List<Table>
        {
            new()
            {
                Id = SeedIds.Table1Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                TableNumber = "1",
                Capacity = 2,
                Location = TableLocation.Indoor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                TableNumber = "2",
                Capacity = 2,
                Location = TableLocation.Indoor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table3Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                TableNumber = "3",
                Capacity = 4,
                Location = TableLocation.Indoor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table4Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                TableNumber = "4",
                Capacity = 6,
                Location = TableLocation.Indoor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table5Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                TableNumber = "5",
                Capacity = 8,
                Location = TableLocation.Outdoor,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.Tables.AddRangeAsync(tables);
        _logger.LogInformation("5 masa eklendi.");
    }

    private async Task SeedTableCombinationsAsync()
    {
        var combo = new TableCombination
        {
            Id = SeedIds.TableCombo1Id,
            TenantId = SeedIds.DemoTenantId,
            VenueId = SeedIds.DemoVenueId,
            Name = "Masa 3+4 Birleşik",
            CombinedCapacity = 10,
            TableIds = new List<Guid> { SeedIds.Table3Id, SeedIds.Table4Id },
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await _context.TableCombinations.AddAsync(combo);
        _logger.LogInformation("1 masa kombinasyonu eklendi.");
    }

    private async Task SeedVenueClosuresAsync()
    {
        var now = DateTime.UtcNow;
        var closures = new List<VenueClosure>
        {
            new()
            {
                Id = SeedIds.VenueClosure1Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Ramazan Bayramı Tatili",
                StartDate = new DateTime(now.Year, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(now.Year, 4, 13, 23, 59, 59, DateTimeKind.Utc),
                IsFullDayClosure = true,
                Reason = "Resmi tatil",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.VenueClosure2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Özel Etkinlik - Erken Kapanış",
                StartDate = new DateTime(now.Year, 5, 15, 21, 0, 0, DateTimeKind.Utc),
                EndDate = new DateTime(now.Year, 5, 15, 23, 0, 0, DateTimeKind.Utc),
                IsFullDayClosure = false,
                Reason = "Özel davet nedeniyle 21:00'de kapanış",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.VenueClosures.AddRangeAsync(closures);
        _logger.LogInformation("2 venue closure kaydı eklendi.");
    }

    private async Task SeedVenueCustomFieldsAsync()
    {
        var customFields = new List<VenueCustomField>
        {
            new()
            {
                Id = SeedIds.CustomField1Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                FieldName = "Doğum günü mü?",
                FieldType = CustomFieldType.Boolean,
                IsRequired = false,
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.CustomField2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                FieldName = "Menü Tercihi",
                FieldType = CustomFieldType.Select,
                IsRequired = false,
                OptionsJson = """
                {
                    "options": ["Klasik", "Vegan", "Vejeteryan", "Glütensiz"]
                }
                """,
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.CustomField3Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                FieldName = "Özel İstek",
                FieldType = CustomFieldType.Text,
                IsRequired = false,
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.VenueCustomFields.AddRangeAsync(customFields);
        _logger.LogInformation("3 custom field eklendi.");
    }

    private async Task SeedRulesAsync()
    {
        var rules = new List<Rule>
        {
            new()
            {
                Id = SeedIds.Rule1Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Erken Rezervasyon İndirimi",
                Description = "7 gün önceden yapılan rezervasyonlara %10 indirim",
                Trigger = RuleTrigger.OnReservationCreate,
                Priority = 10,
                IsActive = true,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "AND",
                    "conditions": [
                        {
                            "field": "DaysUntilReservation",
                            "operator": ">=",
                            "value": 7
                        }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "actions": [
                        {
                            "type": "DISCOUNT",
                            "parameters": {
                                "discountType": "percentage",
                                "discountValue": 10,
                                "reason": "Erken rezervasyon indirimi"
                            }
                        }
                    ]
                }
                """,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Rule2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "VIP Önceliği",
                Description = "VIP müşterilere pencere kenarı masalar öncelikli",
                Trigger = RuleTrigger.OnReservationCreate,
                Priority = 20,
                IsActive = true,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "AND",
                    "conditions": [
                        {
                            "field": "CustomerTier",
                            "operator": "==",
                            "value": "VIP"
                        }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "actions": [
                        {
                            "type": "TABLE_PRIORITY",
                            "parameters": {
                                "preferredLocation": "Window"
                            }
                        }
                    ]
                }
                """,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Rule3Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Büyük Grup Yönlendirmesi",
                Description = "6+ kişilik rezervasyonlar outdoor masalara yönlendirilir",
                Trigger = RuleTrigger.OnReservationCreate,
                Priority = 15,
                IsActive = true,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "AND",
                    "conditions": [
                        {
                            "field": "PartySize",
                            "operator": ">=",
                            "value": 6
                        }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "actions": [
                        {
                            "type": "TABLE_PRIORITY",
                            "parameters": {
                                "preferredLocation": "Outdoor"
                            }
                        }
                    ]
                }
                """,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Rule4Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Hafta Sonu Kapora Zorunluluğu",
                Description = "Cuma/Cumartesi akşam rezervasyonlarda ₺200 kapora gerekli",
                Trigger = RuleTrigger.OnReservationCreate,
                Priority = 30,
                IsActive = true,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "AND",
                    "conditions": [
                        {
                            "field": "DayOfWeek",
                            "operator": "IN",
                            "value": ["Friday", "Saturday"]
                        },
                        {
                            "field": "TimeSlot",
                            "operator": ">=",
                            "value": "19:00"
                        }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "actions": [
                        {
                            "type": "DEPOSIT",
                            "parameters": {
                                "depositAmount": 200,
                                "currency": "TRY",
                                "refundPolicy": "RefundableUntil24Hours"
                            }
                        }
                    ]
                }
                """,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Rule5Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Masa Çevrim Süresi",
                Description = "Aynı masa için 30 dakika arayla yeni rezervasyon alınabilir",
                Trigger = RuleTrigger.OnSlotCheck,
                Priority = 5,
                IsActive = true,
                ConditionsJson = """
                {
                    "version": 1,
                    "operator": "AND",
                    "conditions": [
                        {
                            "field": "MinutesSinceLastReservation",
                            "operator": "<",
                            "value": 30
                        }
                    ]
                }
                """,
                ActionsJson = """
                {
                    "version": 1,
                    "actions": [
                        {
                            "type": "BLOCK_SLOT",
                            "parameters": {
                                "reason": "Masa hazırlık süresi"
                            }
                        }
                    ]
                }
                """,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.Rules.AddRangeAsync(rules);
        _logger.LogInformation("5 kural eklendi.");
    }

    private async Task SeedCustomersAsync()
    {
        var customers = new List<Customer>
        {
            new()
            {
                Id = SeedIds.Customer1Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Mehmet",
                LastName = "Demir",
                Email = "mehmet.demir@example.com",
                PhoneNumber = "+905551001001",
                Tier = CustomerTier.VIP,
                TotalReservations = 25,
                CompletedReservations = 22,
                CancelledReservations = 3,
                NoShowCount = 0,
                TotalSpent = 12500m,
                Notes = "Sık gelen VIP müşterimiz, pencere kenarı masa tercih ediyor",
                CreatedAt = DateTime.UtcNow.AddMonths(-6)
            },
            new()
            {
                Id = SeedIds.Customer2Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Ayşe",
                LastName = "Kaya",
                Email = "ayse.kaya@example.com",
                PhoneNumber = "+905551001002",
                Tier = CustomerTier.Regular,
                TotalReservations = 8,
                CompletedReservations = 7,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 3200m,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new()
            {
                Id = SeedIds.Customer3Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Can",
                LastName = "Özkan",
                Email = "can.ozkan@example.com",
                PhoneNumber = "+905551001003",
                Tier = CustomerTier.New,
                TotalReservations = 2,
                CompletedReservations = 2,
                CancelledReservations = 0,
                NoShowCount = 0,
                TotalSpent = 850m,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Customer4Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Zeynep",
                LastName = "Arslan",
                Email = "zeynep.arslan@example.com",
                PhoneNumber = "+905551001004",
                Tier = CustomerTier.Regular,
                TotalReservations = 12,
                CompletedReservations = 11,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 4800m,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new()
            {
                Id = SeedIds.Customer5Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Emre",
                LastName = "Yıldız",
                Email = "emre.yildiz@example.com",
                PhoneNumber = "+905551001005",
                Tier = CustomerTier.VIP,
                TotalReservations = 18,
                CompletedReservations = 17,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 9200m,
                Notes = "Vegan menü tercihi var",
                CreatedAt = DateTime.UtcNow.AddMonths(-8)
            },
            new()
            {
                Id = SeedIds.Customer6Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Selin",
                LastName = "Çelik",
                Email = "selin.celik@example.com",
                PhoneNumber = "+905551001006",
                Tier = CustomerTier.Regular,
                TotalReservations = 6,
                CompletedReservations = 5,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 2100m,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = SeedIds.Customer7Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Burak",
                LastName = "Koç",
                Email = "burak.koc@example.com",
                PhoneNumber = "+905551001007",
                Tier = CustomerTier.Blocked,
                TotalReservations = 4,
                CompletedReservations = 1,
                CancelledReservations = 1,
                NoShowCount = 2,
                TotalSpent = 350m,
                Notes = "2 kez no-show, bloke edildi",
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            },
            new()
            {
                Id = SeedIds.Customer8Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Elif",
                LastName = "Şahin",
                Email = "elif.sahin@example.com",
                PhoneNumber = "+905551001008",
                Tier = CustomerTier.New,
                TotalReservations = 1,
                CompletedReservations = 1,
                CancelledReservations = 0,
                NoShowCount = 0,
                TotalSpent = 420m,
                CreatedAt = DateTime.UtcNow.AddDays(-15)
            },
            new()
            {
                Id = SeedIds.Customer9Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Cem",
                LastName = "Aydın",
                Email = "cem.aydin@example.com",
                PhoneNumber = "+905551001009",
                Tier = CustomerTier.Regular,
                TotalReservations = 9,
                CompletedReservations = 8,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 3600m,
                CreatedAt = DateTime.UtcNow.AddMonths(-4)
            },
            new()
            {
                Id = SeedIds.Customer10Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Deniz",
                LastName = "Yılmaz",
                Email = "deniz.yilmaz@example.com",
                PhoneNumber = "+905551001010",
                Tier = CustomerTier.VIP,
                TotalReservations = 30,
                CompletedReservations = 28,
                CancelledReservations = 2,
                NoShowCount = 0,
                TotalSpent = 15800m,
                Notes = "Kurumsal müşteri, grup rezervasyonları sık",
                CreatedAt = DateTime.UtcNow.AddMonths(-10)
            },
            new()
            {
                Id = SeedIds.Customer11Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Gizem",
                LastName = "Öztürk",
                Email = "gizem.ozturk@example.com",
                PhoneNumber = "+905551001011",
                Tier = CustomerTier.Regular,
                TotalReservations = 7,
                CompletedReservations = 6,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 2800m,
                CreatedAt = DateTime.UtcNow.AddMonths(-3)
            },
            new()
            {
                Id = SeedIds.Customer12Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Kaan",
                LastName = "Aksoy",
                Email = "kaan.aksoy@example.com",
                PhoneNumber = "+905551001012",
                Tier = CustomerTier.New,
                TotalReservations = 1,
                CompletedReservations = 0,
                CancelledReservations = 0,
                NoShowCount = 0,
                TotalSpent = 0m,
                Notes = "Gelecek hafta için rezervasyon yaptı",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new()
            {
                Id = SeedIds.Customer13Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Nazlı",
                LastName = "Güneş",
                Email = "nazli.gunes@example.com",
                PhoneNumber = "+905551001013",
                Tier = CustomerTier.Regular,
                TotalReservations = 10,
                CompletedReservations = 9,
                CancelledReservations = 1,
                NoShowCount = 0,
                TotalSpent = 4200m,
                CreatedAt = DateTime.UtcNow.AddMonths(-5)
            },
            new()
            {
                Id = SeedIds.Customer14Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Onur",
                LastName = "Kara",
                Email = "onur.kara@example.com",
                PhoneNumber = "+905551001014",
                Tier = CustomerTier.VIP,
                TotalReservations = 22,
                CompletedReservations = 20,
                CancelledReservations = 2,
                NoShowCount = 0,
                TotalSpent = 11500m,
                Notes = "Özel günlerde kapalı masa tercih ediyor",
                CreatedAt = DateTime.UtcNow.AddMonths(-7)
            },
            new()
            {
                Id = SeedIds.Customer15Id,
                TenantId = SeedIds.DemoTenantId,
                FirstName = "Pınar",
                LastName = "Duman",
                Email = "pinar.duman@example.com",
                PhoneNumber = "+905551001015",
                Tier = CustomerTier.Regular,
                TotalReservations = 5,
                CompletedReservations = 5,
                CancelledReservations = 0,
                NoShowCount = 0,
                TotalSpent = 2200m,
                CreatedAt = DateTime.UtcNow.AddMonths(-2)
            }
        };

        await _context.Customers.AddRangeAsync(customers);
        _logger.LogInformation("15 müşteri eklendi.");
    }

    private async Task SeedReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var reservations = new List<Reservation>();

        // Son 30 gün içinde dağıtılmış rezervasyonlar
        var random = new Random(42); // Sabit seed, tutarlılık için
        var customerIds = new[]
        {
            SeedIds.Customer1Id, SeedIds.Customer2Id, SeedIds.Customer3Id,
            SeedIds.Customer4Id, SeedIds.Customer5Id, SeedIds.Customer6Id,
            SeedIds.Customer8Id, SeedIds.Customer9Id, SeedIds.Customer10Id,
            SeedIds.Customer11Id, SeedIds.Customer13Id, SeedIds.Customer14Id, SeedIds.Customer15Id
        };
        var tableIds = new[] { SeedIds.Table1Id, SeedIds.Table2Id, SeedIds.Table3Id, SeedIds.Table4Id, SeedIds.Table5Id };

        for (int i = 0; i < 30; i++)
        {
            var daysAgo = random.Next(1, 31);
            var reservationDate = now.AddDays(-daysAgo).Date.AddHours(19 + random.Next(0, 3));

            var customerId = customerIds[random.Next(customerIds.Length)];
            var tableId = tableIds[random.Next(tableIds.Length)];
            var partySize = random.Next(2, 7);

            var status = daysAgo <= 1 ? ReservationStatus.Pending :
                         daysAgo <= 3 ? (random.Next(0, 10) > 7 ? ReservationStatus.Confirmed : ReservationStatus.Completed) :
                         daysAgo <= 10 ? ReservationStatus.Completed :
                         random.Next(0, 10) > 8 ? ReservationStatus.Cancelled : ReservationStatus.Completed;

            reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                CustomerId = customerId,
                TableId = tableId,
                ReservationDate = reservationDate,
                PartySize = partySize,
                Status = status,
                Source = ReservationSource.Website,
                ConfirmCode = GenerateConfirmCode(),
                Notes = i % 5 == 0 ? "Pencere kenarı masa tercihi" : null,
                CreatedAt = reservationDate.AddDays(-random.Next(1, 5))
            });
        }

        await _context.Reservations.AddRangeAsync(reservations);
        _logger.LogInformation($"{reservations.Count} rezervasyon eklendi.");
    }

    private static string GenerateConfirmCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Karışabilecek karakterler hariç
        var random = new Random();
        return new string(Enumerable.Range(0, 8).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }
}
