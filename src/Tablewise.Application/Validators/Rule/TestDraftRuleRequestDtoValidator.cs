using FluentValidation;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Services;

namespace Tablewise.Application.Validators.Rule;

/// <summary>
/// TestDraftRuleRequestDto için FluentValidation kuralları.
/// </summary>
public sealed class TestDraftRuleRequestDtoValidator : AbstractValidator<TestDraftRuleRequestDto>
{
    /// <summary>
    /// TestDraftRuleRequestDtoValidator constructor.
    /// </summary>
    public TestDraftRuleRequestDtoValidator()
    {
        RuleFor(x => x.RuleType)
            .NotEmpty()
            .WithMessage("Kural tipi zorunludur.");

        RuleFor(x => x.ConditionsJson)
            .NotEmpty()
            .WithMessage("Koşullar JSON zorunludur.")
            .Must(BeValidConditionsJson)
            .WithMessage(x => GetConditionsError(x) ?? "Geçersiz koşullar JSON.");

        RuleFor(x => x.ActionsJson)
            .NotEmpty()
            .WithMessage("Aksiyonlar JSON zorunludur.");

        RuleFor(x => x.Context)
            .NotNull()
            .SetValidator(new TestRuleRequestDtoValidator());
    }

    private static bool BeValidConditionsJson(string? json)
    {
        return RuleSchemaValidator.ValidateConditions(json).IsValid;
    }

    private static string? GetConditionsError(TestDraftRuleRequestDto dto)
    {
        var result = RuleSchemaValidator.ValidateConditions(dto.ConditionsJson);
        return result.IsValid ? null : result.ErrorMessage;
    }
}
