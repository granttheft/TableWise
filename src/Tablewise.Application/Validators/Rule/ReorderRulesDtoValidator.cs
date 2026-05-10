using FluentValidation;
using Tablewise.Application.DTOs.Rule;

namespace Tablewise.Application.Validators.Rule;

/// <summary>
/// ReorderRulesDto için FluentValidation kuralları.
/// </summary>
public sealed class ReorderRulesDtoValidator : AbstractValidator<ReorderRulesDto>
{
    private const int MinPriority = 1;
    private const int MaxPriority = 1000;

    /// <summary>
    /// ReorderRulesDtoValidator constructor.
    /// </summary>
    public ReorderRulesDtoValidator()
    {
        RuleFor(x => x.Rules)
            .NotEmpty().WithMessage("En az bir kural sıralaması belirtilmelidir.")
            .Must(HaveUniqueIds).WithMessage("Kural ID'leri benzersiz olmalıdır.");

        RuleForEach(x => x.Rules)
            .ChildRules(rule =>
            {
                rule.RuleFor(r => r.Id)
                    .NotEqual(Guid.Empty).WithMessage("Geçersiz kural ID.");

                rule.RuleFor(r => r.Priority)
                    .InclusiveBetween(MinPriority, MaxPriority)
                    .WithMessage($"Öncelik {MinPriority}-{MaxPriority} arasında olmalıdır.");
            });
    }

    /// <summary>
    /// Kural ID'lerinin benzersiz olup olmadığını kontrol eder.
    /// </summary>
    private static bool HaveUniqueIds(List<RuleOrderItem>? rules)
    {
        if (rules == null || rules.Count == 0)
            return false;

        var uniqueIds = rules.Select(r => r.Id).Distinct().Count();
        return uniqueIds == rules.Count;
    }
}
