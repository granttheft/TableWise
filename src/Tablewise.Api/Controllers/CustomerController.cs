using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tablewise.Application.DTOs.Customer;
using Tablewise.Application.Features.Customer.Commands;
using Tablewise.Application.Features.Customer.Queries;

namespace Tablewise.Api.Controllers;

/// <summary>
/// Müşteri yönetimi controller'ı.
/// Müşteri listesi, detay, tier ve blacklist işlemleri.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
[Produces("application/json")]
public sealed class CustomerController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CustomerController> _logger;

    /// <summary>
    /// CustomerController constructor.
    /// </summary>
    public CustomerController(
        IMediator mediator,
        ILogger<CustomerController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Müşteri listesini getirir (filtrelenebilir).
    /// </summary>
    /// <param name="search">Arama terimi (isim, email, telefon)</param>
    /// <param name="tier">Tier filtresi</param>
    /// <param name="isBlacklisted">Blacklist durumu</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Müşteri listesi</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCustomers(
        [FromQuery] string? search,
        [FromQuery] string? tier,
        [FromQuery] bool? isBlacklisted,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomersQuery
        {
            SearchTerm = search,
            Tier = tier,
            IsBlacklisted = isBlacklisted
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri arama (typeahead için).
    /// </summary>
    /// <param name="q">Arama terimi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Eşleşen müşteriler</returns>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<CustomerDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchCustomers(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchCustomersQuery { SearchTerm = q };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri detayını getirir.
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Müşteri detayı</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCustomerById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var query = new GetCustomerByIdQuery { CustomerId = id };
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri tier'ını günceller (Owner-only).
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <param name="dto">Tier bilgisi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş müşteri</returns>
    [HttpPatch("{id:guid}/tier")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTier(
        Guid id,
        [FromBody] UpdateCustomerTierDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCustomerTierCommand
        {
            CustomerId = id,
            Tier = dto.Tier
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri blacklist durumunu günceller.
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <param name="dto">Blacklist bilgisi</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş müşteri</returns>
    [HttpPatch("{id:guid}/blacklist")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBlacklist(
        Guid id,
        [FromBody] UpdateCustomerBlacklistDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCustomerBlacklistCommand
        {
            CustomerId = id,
            IsBlacklisted = dto.IsBlacklisted,
            BlacklistReason = dto.BlacklistReason
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Müşteri notlarını günceller.
    /// </summary>
    /// <param name="id">Müşteri ID</param>
    /// <param name="dto">Notlar</param>
    /// <param name="cancellationToken">İptal token'ı</param>
    /// <returns>Güncellenmiş müşteri</returns>
    [HttpPatch("{id:guid}/notes")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateNotes(
        Guid id,
        [FromBody] UpdateCustomerNotesDto dto,
        CancellationToken cancellationToken = default)
    {
        var command = new UpdateCustomerNotesCommand
        {
            CustomerId = id,
            Notes = dto.Notes
        };

        var result = await _mediator.Send(command, cancellationToken);
        return Ok(result);
    }
}
