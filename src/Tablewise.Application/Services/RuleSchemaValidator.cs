using System.Text.Json;

namespace Tablewise.Application.Services;

/// <summary>
/// Kural JSON şema validator.
/// ConditionsJson için field ve operator whitelist kontrolü yapar.
/// </summary>
public static class RuleSchemaValidator
{
    /// <summary>
    /// İzin verilen alan adları (whitelist).
    /// </summary>
    public static readonly HashSet<string> AllowedFields = new(StringComparer.OrdinalIgnoreCase)
    {
        "partySize",
        "daysInAdvance",
        "customer.tier",
        "customer.totalVisits",
        "reservation.reservedFor.hour",
        "reservation.reservedFor.dayOfWeek",
        "venue.currentOccupancy",
        "table.capacity",
        "table.location",
        "groupComposition",
        "femaleRatio",
        "maleRatio",
        "maleCount",
        "femaleCount"
    };

    /// <summary>
    /// İzin verilen operatörler.
    /// </summary>
    public static readonly HashSet<string> AllowedOperators = new(StringComparer.OrdinalIgnoreCase)
    {
        "==", "!=", "<", "<=", ">", ">=", "in", "contains"
    };

    /// <summary>
    /// Geçerli grup kompozisyonu değerleri.
    /// </summary>
    public static readonly HashSet<string> ValidGroupCompositions = new(StringComparer.OrdinalIgnoreCase)
    {
        "Mixed", "AllMale", "AllFemale", "Family"
    };

    /// <summary>
    /// Geçerli müşteri tier değerleri.
    /// </summary>
    public static readonly HashSet<string> ValidCustomerTiers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Regular", "Gold", "VIP", "Blacklisted"
    };

    /// <summary>
    /// Geçerli gün değerleri.
    /// </summary>
    public static readonly HashSet<string> ValidDayOfWeek = new(StringComparer.OrdinalIgnoreCase)
    {
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    };

    /// <summary>
    /// ConditionsJson'ı validate eder.
    /// </summary>
    /// <param name="conditionsJson">Validate edilecek JSON</param>
    /// <returns>Validation sonucu</returns>
    public static RuleSchemaValidationResult ValidateConditions(string? conditionsJson)
    {
        if (string.IsNullOrWhiteSpace(conditionsJson))
        {
            return RuleSchemaValidationResult.Failure("Koşullar boş olamaz.");
        }

        try
        {
            var doc = JsonDocument.Parse(conditionsJson);
            var root = doc.RootElement;

            // Version kontrolü
            if (!root.TryGetProperty("version", out var versionElement) ||
                versionElement.ValueKind != JsonValueKind.Number)
            {
                return RuleSchemaValidationResult.Failure("'version' alanı zorunludur ve sayı olmalıdır.");
            }

            // conditions array kontrolü (opsiyonel - custom_condition için)
            if (root.TryGetProperty("conditions", out var conditionsElement))
            {
                if (conditionsElement.ValueKind != JsonValueKind.Array)
                {
                    return RuleSchemaValidationResult.Failure("'conditions' bir dizi olmalıdır.");
                }

                foreach (var condition in conditionsElement.EnumerateArray())
                {
                    var validationResult = ValidateSingleCondition(condition);
                    if (!validationResult.IsValid)
                    {
                        return validationResult;
                    }
                }
            }

            return RuleSchemaValidationResult.Success();
        }
        catch (JsonException ex)
        {
            return RuleSchemaValidationResult.Failure($"Geçersiz JSON formatı: {ex.Message}");
        }
    }

    /// <summary>
    /// Tek bir condition'ı validate eder.
    /// </summary>
    private static RuleSchemaValidationResult ValidateSingleCondition(JsonElement condition)
    {
        // field kontrolü
        if (!condition.TryGetProperty("field", out var fieldElement) ||
            fieldElement.ValueKind != JsonValueKind.String)
        {
            return RuleSchemaValidationResult.Failure("Her koşulda 'field' alanı zorunludur.");
        }

        var field = fieldElement.GetString() ?? string.Empty;
        if (!AllowedFields.Contains(field))
        {
            return RuleSchemaValidationResult.Failure(
                $"Bilinmeyen alan: '{field}'. İzin verilen alanlar: {string.Join(", ", AllowedFields)}");
        }

        // op kontrolü
        if (!condition.TryGetProperty("op", out var opElement) ||
            opElement.ValueKind != JsonValueKind.String)
        {
            return RuleSchemaValidationResult.Failure("Her koşulda 'op' alanı zorunludur.");
        }

        var op = opElement.GetString() ?? string.Empty;
        if (!AllowedOperators.Contains(op))
        {
            return RuleSchemaValidationResult.Failure(
                $"Bilinmeyen operatör: '{op}'. İzin verilen operatörler: {string.Join(", ", AllowedOperators)}");
        }

        // value kontrolü
        if (!condition.TryGetProperty("value", out var valueElement))
        {
            return RuleSchemaValidationResult.Failure("Her koşulda 'value' alanı zorunludur.");
        }

        // Field'a göre değer validasyonu
        var valueValidation = ValidateFieldValue(field, valueElement, op);
        if (!valueValidation.IsValid)
        {
            return valueValidation;
        }

        return RuleSchemaValidationResult.Success();
    }

    /// <summary>
    /// Alan tipine göre değeri validate eder.
    /// </summary>
    private static RuleSchemaValidationResult ValidateFieldValue(string field, JsonElement value, string op)
    {
        var fieldLower = field.ToLowerInvariant();

        // Ratio alanları için 0.0-1.0 kontrolü
        if (fieldLower is "femaleratio" or "maleratio" or "venue.currentoccupancy")
        {
            if (value.ValueKind == JsonValueKind.Number)
            {
                var numValue = value.GetDouble();
                if (numValue < 0.0 || numValue > 1.0)
                {
                    return RuleSchemaValidationResult.Failure(
                        $"'{field}' alanı için değer 0.0-1.0 arasında olmalıdır. Verilen: {numValue}");
                }
            }
        }

        // GroupComposition kontrolü
        if (fieldLower == "groupcomposition")
        {
            if (op.ToLowerInvariant() == "in")
            {
                // Array içindeki tüm değerler geçerli olmalı
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        var itemStr = item.GetString();
                        if (itemStr != null && !ValidGroupCompositions.Contains(itemStr))
                        {
                            return RuleSchemaValidationResult.Failure(
                                $"Geçersiz grup kompozisyonu: '{itemStr}'. Geçerli değerler: {string.Join(", ", ValidGroupCompositions)}");
                        }
                    }
                }
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                var strValue = value.GetString();
                if (strValue != null && !ValidGroupCompositions.Contains(strValue))
                {
                    return RuleSchemaValidationResult.Failure(
                        $"Geçersiz grup kompozisyonu: '{strValue}'. Geçerli değerler: {string.Join(", ", ValidGroupCompositions)}");
                }
            }
        }

        // CustomerTier kontrolü
        if (fieldLower == "customer.tier")
        {
            if (op.ToLowerInvariant() == "in")
            {
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        var itemStr = item.GetString();
                        if (itemStr != null && !ValidCustomerTiers.Contains(itemStr))
                        {
                            return RuleSchemaValidationResult.Failure(
                                $"Geçersiz müşteri tier: '{itemStr}'. Geçerli değerler: {string.Join(", ", ValidCustomerTiers)}");
                        }
                    }
                }
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                var strValue = value.GetString();
                if (strValue != null && !ValidCustomerTiers.Contains(strValue))
                {
                    return RuleSchemaValidationResult.Failure(
                        $"Geçersiz müşteri tier: '{strValue}'. Geçerli değerler: {string.Join(", ", ValidCustomerTiers)}");
                }
            }
        }

        // DayOfWeek kontrolü
        if (fieldLower == "reservation.reservedfor.dayofweek")
        {
            if (op.ToLowerInvariant() == "in")
            {
                if (value.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in value.EnumerateArray())
                    {
                        var itemStr = item.GetString();
                        if (itemStr != null && !ValidDayOfWeek.Contains(itemStr))
                        {
                            return RuleSchemaValidationResult.Failure(
                                $"Geçersiz gün: '{itemStr}'. Geçerli değerler: {string.Join(", ", ValidDayOfWeek)}");
                        }
                    }
                }
            }
            else if (value.ValueKind == JsonValueKind.String)
            {
                var strValue = value.GetString();
                if (strValue != null && !ValidDayOfWeek.Contains(strValue))
                {
                    return RuleSchemaValidationResult.Failure(
                        $"Geçersiz gün: '{strValue}'. Geçerli değerler: {string.Join(", ", ValidDayOfWeek)}");
                }
            }
        }

        // Hour kontrolü (0-23)
        if (fieldLower == "reservation.reservedfor.hour")
        {
            if (value.ValueKind == JsonValueKind.Number)
            {
                var hour = value.GetInt32();
                if (hour < 0 || hour > 23)
                {
                    return RuleSchemaValidationResult.Failure(
                        $"Saat değeri 0-23 arasında olmalıdır. Verilen: {hour}");
                }
            }
        }

        return RuleSchemaValidationResult.Success();
    }
}

/// <summary>
/// Kural şema validasyon sonucu.
/// </summary>
public sealed class RuleSchemaValidationResult
{
    /// <summary>
    /// Validasyon başarılı mı?
    /// </summary>
    public bool IsValid { get; init; }

    /// <summary>
    /// Hata mesajı (IsValid=false ise).
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Başarılı sonuç oluşturur.
    /// </summary>
    public static RuleSchemaValidationResult Success() => new() { IsValid = true };

    /// <summary>
    /// Başarısız sonuç oluşturur.
    /// </summary>
    public static RuleSchemaValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        ErrorMessage = errorMessage
    };
}
