using FluentValidation;
using Tablewise.Application.DTOs.Booking;

namespace Tablewise.Application.Validators.Booking;

/// <summary>
/// CancelReservationRequestDto için FluentValidation kuralları.
/// Rezervasyon iptal endpoint validasyonu.
/// </summary>
public sealed class CancelReservationRequestDtoValidator : AbstractValidator<CancelReservationRequestDto>
{
    private const int MaxReasonLength = 500;

    /// <summary>
    /// CancelReservationRequestDtoValidator constructor.
    /// </summary>
    public CancelReservationRequestDtoValidator()
    {
        RuleFor(x => x.Reason)
            .MaximumLength(MaxReasonLength)
            .WithMessage($"İptal nedeni en fazla {MaxReasonLength} karakter olabilir.")
            .When(x => !string.IsNullOrEmpty(x.Reason));
    }
}
