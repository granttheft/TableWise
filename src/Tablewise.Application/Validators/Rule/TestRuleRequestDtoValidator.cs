using FluentValidation;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Services;

namespace Tablewise.Application.Validators.Rule;

/// <summary>
/// TestRuleRequestDto için FluentValidation kuralları.
/// </summary>
public sealed class TestRuleRequestDtoValidator : AbstractValidator<TestRuleRequestDto>
{
    private const int MinPartySize = 1;
    private const int MaxPartySize = 50;
    private const int MinHour = 0;
    private const int MaxHour = 23;

    /// <summary>
    /// TestRuleRequestDtoValidator constructor.
    /// </summary>
    public TestRuleRequestDtoValidator()
    {
        RuleFor(x => x.PartySize)
            .InclusiveBetween(MinPartySize, MaxPartySize)
            .WithMessage($"Kişi sayısı {MinPartySize}-{MaxPartySize} arasında olmalıdır.");

        RuleFor(x => x.DaysInAdvance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Gün sayısı 0 veya daha büyük olmalıdır.");

        RuleFor(x => x.Hour)
            .InclusiveBetween(MinHour, MaxHour)
            .WithMessage($"Saat {MinHour}-{MaxHour} arasında olmalıdır.");

        RuleFor(x => x.VenueOccupancy)
            .InclusiveBetween(0.0, 1.0)
            .WithMessage("Doluluk oranı 0.0-1.0 arasında olmalıdır.");

        RuleFor(x => x.CustomerTier)
            .Must(tier => tier == null || RuleSchemaValidator.ValidCustomerTiers.Contains(tier))
            .WithMessage($"Geçersiz müşteri tier. Geçerli değerler: {string.Join(", ", RuleSchemaValidator.ValidCustomerTiers)}");

        RuleFor(x => x.DayOfWeek)
            .Must(day => day == null || RuleSchemaValidator.ValidDayOfWeek.Contains(day))
            .WithMessage($"Geçersiz gün. Geçerli değerler: {string.Join(", ", RuleSchemaValidator.ValidDayOfWeek)}");

        RuleFor(x => x.GroupComposition)
            .Must(gc => gc == null || RuleSchemaValidator.ValidGroupCompositions.Contains(gc))
            .WithMessage($"Geçersiz grup kompozisyonu. Geçerli değerler: {string.Join(", ", RuleSchemaValidator.ValidGroupCompositions)}");

        RuleFor(x => x.TableCapacity)
            .GreaterThan(0).WithMessage("Masa kapasitesi 0'dan büyük olmalıdır.")
            .When(x => x.TableCapacity.HasValue);

        RuleFor(x => x.MaleCount)
            .GreaterThanOrEqualTo(0).WithMessage("Erkek sayısı 0 veya daha büyük olmalıdır.")
            .When(x => x.MaleCount.HasValue);

        RuleFor(x => x.FemaleCount)
            .GreaterThanOrEqualTo(0).WithMessage("Kadın sayısı 0 veya daha büyük olmalıdır.")
            .When(x => x.FemaleCount.HasValue);

        // MaleCount + FemaleCount <= PartySize kontrolü
        RuleFor(x => x)
            .Must(dto =>
            {
                var totalGender = (dto.MaleCount ?? 0) + (dto.FemaleCount ?? 0);
                return totalGender <= dto.PartySize;
            })
            .WithMessage("Erkek ve kadın sayısı toplamı kişi sayısını geçemez.")
            .When(x => x.MaleCount.HasValue || x.FemaleCount.HasValue);
    }
}
