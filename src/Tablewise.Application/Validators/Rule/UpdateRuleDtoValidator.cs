using System.Text.Json;
using FluentValidation;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Services;

namespace Tablewise.Application.Validators.Rule;

/// <summary>
/// UpdateRuleDto için FluentValidation kuralları.
/// </summary>
public sealed class UpdateRuleDtoValidator : AbstractValidator<UpdateRuleDto>
{
    private const int MinNameLength = 2;
    private const int MaxNameLength = 200;
    private const int MinPriority = 1;
    private const int MaxPriority = 1000;

    /// <summary>
    /// UpdateRuleDtoValidator constructor.
    /// </summary>
    public UpdateRuleDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Kural adı zorunludur.")
            .MinimumLength(MinNameLength).WithMessage($"Kural adı en az {MinNameLength} karakter olmalıdır.")
            .MaximumLength(MaxNameLength).WithMessage($"Kural adı en fazla {MaxNameLength} karakter olabilir.");

        RuleFor(x => x.RuleType)
            .NotEmpty().WithMessage("Kural tipi zorunludur.")
            .MaximumLength(100).WithMessage("Kural tipi en fazla 100 karakter olabilir.");

        RuleFor(x => x.ConditionsJson)
            .NotEmpty().WithMessage("Koşullar zorunludur.")
            .Must(BeValidJson).WithMessage("Koşullar geçersiz JSON formatında.")
            .Must(HaveVersionField).WithMessage("Koşullar JSON'unda 'version' alanı zorunludur.")
            .Must(BeValidConditionsSchema).WithMessage(GetConditionsSchemaError);

        RuleFor(x => x.ActionsJson)
            .NotEmpty().WithMessage("Aksiyonlar zorunludur.")
            .Must(BeValidJson).WithMessage("Aksiyonlar geçersiz JSON formatında.")
            .Must(HaveVersionField).WithMessage("Aksiyonlar JSON'unda 'version' alanı zorunludur.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(MinPriority, MaxPriority)
            .WithMessage($"Öncelik {MinPriority}-{MaxPriority} arasında olmalıdır.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Açıklama en fazla 500 karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.ApplicableTimeSlots)
            .Must(BeValidJson).WithMessage("Uygulanabilir zaman dilimleri geçersiz JSON formatında.")
            .When(x => !string.IsNullOrEmpty(x.ApplicableTimeSlots));
    }

    /// <summary>
    /// Geçerli JSON olup olmadığını kontrol eder.
    /// </summary>
    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// JSON'da version alanı olup olmadığını kontrol eder.
    /// </summary>
    private static bool HaveVersionField(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("version", out _);
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// ConditionsJson şema validasyonu.
    /// Field ve operator whitelist kontrolü yapar.
    /// </summary>
    private static bool BeValidConditionsSchema(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return true; // Diğer validator'lar zaten kontrol ediyor

        var result = RuleSchemaValidator.ValidateConditions(json);
        return result.IsValid;
    }

    /// <summary>
    /// ConditionsJson şema validasyonu hata mesajını döner.
    /// </summary>
    private static string GetConditionsSchemaError(UpdateRuleDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.ConditionsJson))
            return "Koşullar zorunludur.";

        var result = RuleSchemaValidator.ValidateConditions(dto.ConditionsJson);
        return result.ErrorMessage ?? "Koşullar şeması geçersiz.";
    }
}
