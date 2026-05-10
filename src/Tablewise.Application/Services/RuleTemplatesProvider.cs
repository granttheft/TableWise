using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Services;

/// <summary>
/// Kural şablonları sağlayıcısı.
/// Hazır kural şablonlarını statik olarak tutar.
/// </summary>
public static class RuleTemplatesProvider
{
    private static readonly List<RuleTemplateDto> Templates =
    [
        new RuleTemplateDto
        {
            Id = "early_booking",
            Name = "Erken Rezervasyon İndirimi",
            Description = "Belirtilen gün sayısı öncesinden yapılan rezervasyonlara indirim uygular",
            Icon = "calendar-clock",
            Category = "Promotion",
            DefaultConditionsJson = """{"version":1,"daysBefore":7}""",
            DefaultActionsJson = """{"version":1,"discountPercent":10}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "daysBefore", new { type = "number", label = "Kaç gün öncesinden", min = 1, max = 90, defaultValue = 7 } },
                { "discountPercent", new { type = "number", label = "İndirim yüzdesi", min = 5, max = 50, defaultValue = 10 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "vip_priority",
            Name = "VIP Öncelik",
            Description = "VIP müşterilere öncelik tanır ve özel muamele sağlar",
            Icon = "crown",
            Category = "Customer",
            DefaultConditionsJson = """{"version":1,"customerTier":"VIP"}""",
            DefaultActionsJson = """{"version":1,"priority":"high","specialTable":true}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "customerTier", new { type = "select", label = "Müşteri seviyesi", options = new[] { "Gold", "VIP" }, defaultValue = "VIP" } },
                { "discountPercent", new { type = "number", label = "İndirim (opsiyonel)", min = 0, max = 30, defaultValue = 0 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "large_group",
            Name = "Büyük Grup",
            Description = "Belirtilen kişi sayısından fazla gruplara özel politika uygular",
            Icon = "users",
            Category = "Group",
            DefaultConditionsJson = """{"version":1,"minPartySize":8}""",
            DefaultActionsJson = """{"version":1,"requiresDeposit":true,"depositPercent":30}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "minPartySize", new { type = "number", label = "Minimum kişi sayısı", min = 4, max = 50, defaultValue = 8 } },
                { "requiresDeposit", new { type = "boolean", label = "Kapora zorunlu", defaultValue = true } },
                { "depositPercent", new { type = "number", label = "Kapora yüzdesi", min = 10, max = 100, defaultValue = 30 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "deposit_required",
            Name = "Kapora Zorunluluğu",
            Description = "Belirli koşullarda kapora talep eder",
            Icon = "credit-card",
            Category = "Payment",
            DefaultConditionsJson = """{"version":1,"dayOfWeek":[6,7],"timeSlot":"19:00-23:00"}""",
            DefaultActionsJson = """{"version":1,"requiresDeposit":true,"depositAmount":100}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "dayOfWeek", new { type = "multiselect", label = "Haftanın günleri", options = new[] { "1:Pazartesi", "2:Salı", "3:Çarşamba", "4:Perşembe", "5:Cuma", "6:Cumartesi", "7:Pazar" } } },
                { "timeSlot", new { type = "text", label = "Saat aralığı (örn: 19:00-23:00)", defaultValue = "19:00-23:00" } },
                { "depositAmount", new { type = "number", label = "Kapora tutarı (TL)", min = 50, max = 1000, defaultValue = 100 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "peak_hour",
            Name = "Yoğun Saat",
            Description = "Yoğun saatlerde farklı politika uygular",
            Icon = "clock",
            Category = "TimeSlot",
            DefaultConditionsJson = """{"version":1,"timeSlot":"19:00-22:00"}""",
            DefaultActionsJson = """{"version":1,"minDuration":120,"noDiscount":true}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "timeSlot", new { type = "text", label = "Yoğun saat aralığı", defaultValue = "19:00-22:00" } },
                { "minDuration", new { type = "number", label = "Minimum süre (dakika)", min = 60, max = 240, defaultValue = 120 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "min_duration",
            Name = "Minimum Süre",
            Description = "Rezervasyonlarda minimum kalış süresi gerektirir",
            Icon = "timer",
            Category = "TimeSlot",
            DefaultConditionsJson = """{"version":1,"partySize":6}""",
            DefaultActionsJson = """{"version":1,"minDurationMinutes":120}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "partySize", new { type = "number", label = "Minimum kişi sayısı", min = 1, max = 20, defaultValue = 6 } },
                { "minDurationMinutes", new { type = "number", label = "Minimum süre (dakika)", min = 60, max = 360, defaultValue = 120 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "blacklist",
            Name = "Kara Liste",
            Description = "Kara listedeki müşterilerin rezervasyon yapmasını engeller",
            Icon = "ban",
            Category = "Customer",
            DefaultConditionsJson = """{"version":1,"customerTier":"Blacklisted"}""",
            DefaultActionsJson = """{"version":1,"block":true,"blockReason":"Müşteri kara listede"}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "blockReason", new { type = "text", label = "Engelleme nedeni", defaultValue = "Müşteri kara listede" } }
            }
        },

        new RuleTemplateDto
        {
            Id = "special_day",
            Name = "Özel Gün",
            Description = "Özel günlerde farklı politika uygular (sevgililer günü, yılbaşı vb.)",
            Icon = "star",
            Category = "Special",
            DefaultConditionsJson = """{"version":1,"specificDates":["2026-12-31","2027-02-14"]}""",
            DefaultActionsJson = """{"version":1,"requiresDeposit":true,"depositAmount":200,"minSpend":500}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "specificDates", new { type = "datelist", label = "Özel tarihler" } },
                { "depositAmount", new { type = "number", label = "Kapora tutarı (TL)", min = 100, max = 1000, defaultValue = 200 } },
                { "minSpend", new { type = "number", label = "Minimum harcama (TL)", min = 0, max = 5000, defaultValue = 500 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "table_cooldown",
            Name = "Masa Bekleme Süresi",
            Description = "Aynı masanın ardışık rezervasyonları arasında bekleme süresi koyar",
            Icon = "pause",
            Category = "Table",
            DefaultConditionsJson = """{"version":1,"cooldownMinutes":30}""",
            DefaultActionsJson = """{"version":1,"enforceGap":true}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "cooldownMinutes", new { type = "number", label = "Bekleme süresi (dakika)", min = 15, max = 120, defaultValue = 30 } }
            }
        },

        new RuleTemplateDto
        {
            Id = "min_spend",
            Name = "Minimum Harcama",
            Description = "Belirli koşullarda minimum harcama şartı getirir",
            Icon = "dollar-sign",
            Category = "Payment",
            DefaultConditionsJson = """{"version":1,"partySize":4,"dayOfWeek":[6,7]}""",
            DefaultActionsJson = """{"version":1,"minSpendAmount":300}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "partySize", new { type = "number", label = "Minimum kişi sayısı", min = 1, max = 20, defaultValue = 4 } },
                { "minSpendAmount", new { type = "number", label = "Minimum harcama (TL)", min = 100, max = 5000, defaultValue = 300 } },
                { "dayOfWeek", new { type = "multiselect", label = "Haftanın günleri", options = new[] { "1:Pazartesi", "2:Salı", "3:Çarşamba", "4:Perşembe", "5:Cuma", "6:Cumartesi", "7:Pazar" } } }
            }
        },

        new RuleTemplateDto
        {
            Id = "custom_condition",
            Name = "Özel Koşul",
            Description = "Tamamen özelleştirilebilir kural. Manuel JSON düzenleme gerektirir.",
            Icon = "code",
            Category = "Advanced",
            DefaultConditionsJson = """{"version":1,"customCondition":{}}""",
            DefaultActionsJson = """{"version":1,"customAction":{}}""",
            ParamsSchema = new Dictionary<string, object>
            {
                { "note", new { type = "info", label = "Bu şablon manuel JSON düzenleme gerektirir" } }
            }
        }
    ];

    /// <summary>
    /// Tüm şablonları getirir.
    /// </summary>
    /// <returns>Şablon listesi</returns>
    public static List<RuleTemplateDto> GetAll() => Templates;

    /// <summary>
    /// ID'ye göre şablon getirir.
    /// </summary>
    /// <param name="id">Şablon ID</param>
    /// <returns>Şablon veya null</returns>
    public static RuleTemplateDto? GetById(string id) =>
        Templates.FirstOrDefault(t => t.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Kategoriye göre şablonları getirir.
    /// </summary>
    /// <param name="category">Kategori adı</param>
    /// <returns>Şablon listesi</returns>
    public static List<RuleTemplateDto> GetByCategory(string category) =>
        Templates.Where(t => t.Category.Equals(category, StringComparison.OrdinalIgnoreCase)).ToList();
}
