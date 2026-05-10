using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Enums;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Kural test komutu handler'ı.
/// </summary>
public sealed class TestRuleCommandHandler : IRequestHandler<TestRuleCommand, RuleTestResultDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUser _currentUser;
    private readonly IRuleTestService _ruleTestService;
    private readonly ILogger<TestRuleCommandHandler> _logger;

    public TestRuleCommandHandler(
        IApplicationDbContext dbContext,
        ITenantContext tenantContext,
        ICurrentUser currentUser,
        IRuleTestService ruleTestService,
        ILogger<TestRuleCommandHandler> logger)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
        _currentUser = currentUser;
        _ruleTestService = ruleTestService;
        _logger = logger;
    }

    public async Task<RuleTestResultDto> Handle(TestRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId;

        // Yetki kontrolü - sadece Owner
        if (_currentUser.Role != UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural test edebilir.");
        }

        // Rule varlık kontrolü
        var ruleExists = await _dbContext.Rules
            .AnyAsync(r => r.Id == request.RuleId && r.TenantId == tenantId && !r.IsDeleted, cancellationToken)
            .ConfigureAwait(false);

        if (!ruleExists)
        {
            throw new NotFoundException("Rule", request.RuleId);
        }

        // Test servisi çağır
        var result = await _ruleTestService.TestRuleAsync(request.RuleId, request.Dto, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Kural test edildi: RuleId={RuleId}, Triggered={Triggered}",
            request.RuleId, result.Triggered);

        return result;
    }
}
