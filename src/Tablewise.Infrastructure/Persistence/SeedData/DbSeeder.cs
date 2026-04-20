using System.Security.Cryptography;
using System.Text.Json;
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
    private const int DemoVenueSlotDurationMinutes = 90;

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
    /// <returns>Asenkron görev.</returns>
    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Seed data işlemi başlıyor...");

            var demoTenantExists = await _context.Tenants
                .IgnoreQueryFilters()
                .AnyAsync(t => t.Id == SeedIds.DemoTenantId);

            if (demoTenantExists)
            {
                _logger.LogInformation("Demo veriler zaten mevcut, seed atlanıyor.");
                return;
            }

            await SeedPlansAsync();
            await SeedPlatformTenantAsync();
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
                MonthlyPriceTry = 0m,
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

    /// <summary>
    /// SuperAdmin için teknik tenant oluşturur (User FK ve TenantScopedEntity için zorunlu).
    /// </summary>
    private async Task SeedPlatformTenantAsync()
    {
        var platformTenant = new Tenant
        {
            Id = SeedIds.PlatformTenantId,
            Name = "Tablewise Platform",
            Slug = "platform-internal",
            Email = "platform-internal@tablewise.local",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("PlatformTenant-NotForLogin-ChangeMe"),
            PlanId = SeedIds.PlanEnterpriseId,
            PlanStatus = PlanStatus.Active,
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        await _context.Tenants.AddAsync(platformTenant);
        _logger.LogInformation("Platform tenant eklendi.");
    }

    /// <summary>
    /// Süper admin kullanıcısı ekler (şifre: SuperAdmin123!).
    /// </summary>
    private async Task SeedSuperAdminAsync()
    {
        var superAdmin = new User
        {
            Id = SeedIds.SuperAdminUserId,
            TenantId = SeedIds.PlatformTenantId,
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
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("TenantPortal-NotUsed-ChangeMe"),
            PlanId = SeedIds.PlanProId,
            PlanStatus = PlanStatus.Active,
            TrialEndsAt = DateTime.UtcNow.AddDays(-15),
            PlanRenewsAt = DateTime.UtcNow.AddMonths(11),
            IsActive = true,
            IsEmailVerified = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        var subscription = new Subscription
        {
            Id = SeedIds.DemoSubscriptionId,
            TenantId = SeedIds.DemoTenantId,
            PlanId = SeedIds.PlanProId,
            Status = PlanStatus.Active,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow.AddMonths(11),
            Amount = 9900m,
            Currency = "TRY",
            NextBillingDate = DateTime.UtcNow.AddMonths(11),
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

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
            Description = "Bağdat Caddesi'ndeki ana şubemiz",
            Address = "Bağdat Caddesi No:123, Kadıköy, İstanbul",
            PhoneNumber = "+905551112233",
            TimeZone = "Europe/Istanbul",
            OpeningTime = new TimeSpan(12, 0, 0),
            ClosingTime = new TimeSpan(23, 0, 0),
            SlotDurationMinutes = DemoVenueSlotDurationMinutes,
            DepositRefundPolicy = DepositRefundPolicy.FullRefund,
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
                Name = "Masa 1",
                Capacity = 2,
                Location = TableLocation.Indoor,
                SortOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Masa 2",
                Capacity = 2,
                Location = TableLocation.Indoor,
                SortOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table3Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Masa 3",
                Capacity = 4,
                Location = TableLocation.Indoor,
                SortOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table4Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Masa 4",
                Capacity = 6,
                Location = TableLocation.Indoor,
                SortOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Table5Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Name = "Masa 5",
                Capacity = 8,
                Location = TableLocation.Outdoor,
                SortOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            }
        };

        await _context.Tables.AddRangeAsync(tables);
        _logger.LogInformation("5 masa eklendi.");
    }

    private async Task SeedTableCombinationsAsync()
    {
        var tableIdsJson = JsonSerializer.Serialize(new[] { SeedIds.Table3Id, SeedIds.Table4Id });

        var combo = new TableCombination
        {
            Id = SeedIds.TableCombo1Id,
            TenantId = SeedIds.DemoTenantId,
            VenueId = SeedIds.DemoVenueId,
            Name = "Masa 3+4 Birleşik",
            CombinedCapacity = 10,
            TableIds = tableIdsJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddMonths(-1)
        };

        await _context.TableCombinations.AddAsync(combo);
        _logger.LogInformation("1 masa kombinasyonu eklendi.");
    }

    private async Task SeedVenueClosuresAsync()
    {
        var year = DateTime.UtcNow.Year;
        var closures = new List<VenueClosure>
        {
            new()
            {
                Id = SeedIds.VenueClosure1Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Date = new DateTime(year, 4, 10, 0, 0, 0, DateTimeKind.Utc),
                IsFullDay = true,
                Reason = "Ramazan Bayramı — tam gün kapalı (örnek)",
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.VenueClosure2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Date = new DateTime(year, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                IsFullDay = false,
                OpenTime = new TimeSpan(12, 0, 0),
                CloseTime = new TimeSpan(21, 0, 0),
                Reason = "Özel etkinlik — 21:00'e kadar açık (örnek kısmi gün)",
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
                Label = "Doğum günü mü?",
                FieldType = CustomFieldType.Boolean,
                IsRequired = false,
                SortOrder = 1,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.CustomField2Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Label = "Menü tercihi",
                FieldType = CustomFieldType.Select,
                IsRequired = false,
                Options = """["Klasik","Vegan","Vejeteryan","Glütensiz"]""",
                SortOrder = 2,
                CreatedAt = DateTime.UtcNow.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.CustomField3Id,
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                Label = "Özel istek",
                FieldType = CustomFieldType.Text,
                IsRequired = false,
                SortOrder = 3,
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
                RuleType = "EarlyBooking",
                TriggerType = RuleTrigger.OnReservationCreate,
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
                RuleType = "VIPPriority",
                TriggerType = RuleTrigger.OnReservationCreate,
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
                RuleType = "LargeGroupRouting",
                TriggerType = RuleTrigger.OnReservationCreate,
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
                RuleType = "DepositRequired",
                TriggerType = RuleTrigger.OnReservationCreate,
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
                Description = "Masa atama aşamasında çevrim süresi kontrolü (örnek kural)",
                RuleType = "TableTurnover",
                TriggerType = RuleTrigger.OnSeatAssign,
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
        var seedTime = DateTime.UtcNow.AddMonths(-1);
        var customers = new List<Customer>
        {
            new()
            {
                Id = SeedIds.Customer1Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Mehmet Demir",
                Phone = "+905551001001",
                Email = "mehmet.demir@example.com",
                Tier = CustomerTier.VIP,
                TotalVisits = 22,
                Notes = "Sık gelen VIP müşteri; pencere kenarı tercihi",
                CreatedAt = seedTime.AddMonths(-5)
            },
            new()
            {
                Id = SeedIds.Customer2Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Ayşe Kaya",
                Phone = "+905551001002",
                Email = "ayse.kaya@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 7,
                CreatedAt = seedTime.AddMonths(-3)
            },
            new()
            {
                Id = SeedIds.Customer3Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Can Özkan",
                Phone = "+905551001003",
                Email = "can.ozkan@example.com",
                Tier = CustomerTier.Gold,
                TotalVisits = 2,
                CreatedAt = seedTime
            },
            new()
            {
                Id = SeedIds.Customer4Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Zeynep Arslan",
                Phone = "+905551001004",
                Email = "zeynep.arslan@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 11,
                CreatedAt = seedTime.AddMonths(-4)
            },
            new()
            {
                Id = SeedIds.Customer5Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Emre Yıldız",
                Phone = "+905551001005",
                Email = "emre.yildiz@example.com",
                Tier = CustomerTier.VIP,
                TotalVisits = 17,
                Notes = "Vegan menü tercihi",
                CreatedAt = seedTime.AddMonths(-7)
            },
            new()
            {
                Id = SeedIds.Customer6Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Selin Çelik",
                Phone = "+905551001006",
                Email = "selin.celik@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 5,
                CreatedAt = seedTime.AddMonths(-2)
            },
            new()
            {
                Id = SeedIds.Customer7Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Burak Koç",
                Phone = "+905551001007",
                Email = "burak.koc@example.com",
                Tier = CustomerTier.Blacklisted,
                IsBlacklisted = true,
                BlacklistReason = "Tekrarlayan no-show",
                TotalVisits = 1,
                Notes = "Kara liste (örnek)",
                CreatedAt = seedTime.AddMonths(-1)
            },
            new()
            {
                Id = SeedIds.Customer8Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Elif Şahin",
                Phone = "+905551001008",
                Email = "elif.sahin@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 1,
                CreatedAt = seedTime.AddDays(-14)
            },
            new()
            {
                Id = SeedIds.Customer9Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Cem Aydın",
                Phone = "+905551001009",
                Email = "cem.aydin@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 8,
                CreatedAt = seedTime.AddMonths(-3)
            },
            new()
            {
                Id = SeedIds.Customer10Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Deniz Yılmaz",
                Phone = "+905551001010",
                Email = "deniz.yilmaz@example.com",
                Tier = CustomerTier.VIP,
                TotalVisits = 28,
                Notes = "Kurumsal müşteri; grup rezervasyonları sık",
                CreatedAt = seedTime.AddMonths(-9)
            },
            new()
            {
                Id = SeedIds.Customer11Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Gizem Öztürk",
                Phone = "+905551001011",
                Email = "gizem.ozturk@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 6,
                CreatedAt = seedTime.AddMonths(-2)
            },
            new()
            {
                Id = SeedIds.Customer12Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Kaan Aksoy",
                Phone = "+905551001012",
                Email = "kaan.aksoy@example.com",
                Tier = CustomerTier.Gold,
                TotalVisits = 0,
                Notes = "Yeni müşteri (örnek)",
                CreatedAt = seedTime.AddDays(-2)
            },
            new()
            {
                Id = SeedIds.Customer13Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Nazlı Güneş",
                Phone = "+905551001013",
                Email = "nazli.gunes@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 9,
                CreatedAt = seedTime.AddMonths(-4)
            },
            new()
            {
                Id = SeedIds.Customer14Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Onur Kara",
                Phone = "+905551001014",
                Email = "onur.kara@example.com",
                Tier = CustomerTier.VIP,
                TotalVisits = 20,
                Notes = "Özel günlerde kapalı masa tercihi",
                CreatedAt = seedTime.AddMonths(-6)
            },
            new()
            {
                Id = SeedIds.Customer15Id,
                TenantId = SeedIds.DemoTenantId,
                FullName = "Pınar Duman",
                Phone = "+905551001015",
                Email = "pinar.duman@example.com",
                Tier = CustomerTier.Regular,
                TotalVisits = 5,
                CreatedAt = seedTime.AddMonths(-1)
            }
        };

        await _context.Customers.AddRangeAsync(customers);
        _logger.LogInformation("15 müşteri eklendi.");
    }

    private async Task SeedReservationsAsync()
    {
        var now = DateTime.UtcNow;
        var reservations = new List<Reservation>();

        var customerProfiles = new (Guid Id, string FullName, string Phone)[]
        {
            (SeedIds.Customer1Id, "Mehmet Demir", "+905551001001"),
            (SeedIds.Customer2Id, "Ayşe Kaya", "+905551001002"),
            (SeedIds.Customer3Id, "Can Özkan", "+905551001003"),
            (SeedIds.Customer4Id, "Zeynep Arslan", "+905551001004"),
            (SeedIds.Customer5Id, "Emre Yıldız", "+905551001005"),
            (SeedIds.Customer6Id, "Selin Çelik", "+905551001006"),
            (SeedIds.Customer8Id, "Elif Şahin", "+905551001008"),
            (SeedIds.Customer9Id, "Cem Aydın", "+905551001009"),
            (SeedIds.Customer10Id, "Deniz Yılmaz", "+905551001010"),
            (SeedIds.Customer11Id, "Gizem Öztürk", "+905551001011"),
            (SeedIds.Customer13Id, "Nazlı Güneş", "+905551001013"),
            (SeedIds.Customer14Id, "Onur Kara", "+905551001014"),
            (SeedIds.Customer15Id, "Pınar Duman", "+905551001015")
        };

        var tableIds = new[] { SeedIds.Table1Id, SeedIds.Table2Id, SeedIds.Table3Id, SeedIds.Table4Id, SeedIds.Table5Id };

        var random = new Random(42);
        for (var i = 0; i < 30; i++)
        {
            var daysAgo = random.Next(1, 31);
            var reservedFor = now.AddDays(-daysAgo).Date.AddHours(19 + random.Next(0, 3));

            var profile = customerProfiles[random.Next(customerProfiles.Length)];
            var tableId = tableIds[random.Next(tableIds.Length)];
            var partySize = random.Next(2, 7);

            var status = daysAgo <= 1
                ? ReservationStatus.Pending
                : daysAgo <= 3
                    ? random.Next(0, 10) > 7 ? ReservationStatus.Confirmed : ReservationStatus.Completed
                    : daysAgo <= 10
                        ? ReservationStatus.Completed
                        : random.Next(0, 10) > 8 ? ReservationStatus.Cancelled : ReservationStatus.Completed;

            reservations.Add(new Reservation
            {
                Id = Guid.NewGuid(),
                TenantId = SeedIds.DemoTenantId,
                VenueId = SeedIds.DemoVenueId,
                CustomerId = profile.Id,
                TableId = tableId,
                GuestName = profile.FullName,
                GuestPhone = profile.Phone,
                PartySize = partySize,
                ReservedFor = reservedFor,
                EndTime = reservedFor.AddMinutes(DemoVenueSlotDurationMinutes),
                Status = status,
                Source = ReservationSource.BookingUI,
                ConfirmCode = GenerateConfirmCode(),
                SpecialRequests = i % 5 == 0 ? "Pencere kenarı masa tercihi" : null,
                CreatedAt = reservedFor.AddDays(-random.Next(1, 5))
            });
        }

        await _context.Reservations.AddRangeAsync(reservations);
        _logger.LogInformation("{Count} rezervasyon eklendi.", reservations.Count);
    }

    /// <summary>
    /// 8 karakterlik, kriptografik olarak güvenli onay kodu üretir (O/0 ve I/1 hariç alfabe).
    /// </summary>
    /// <returns>Onay kodu.</returns>
    private static string GenerateConfirmCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        Span<char> result = stackalloc char[8];
        var bytes = new byte[8];
        RandomNumberGenerator.Fill(bytes);
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }

        return new string(result);
    }
}
