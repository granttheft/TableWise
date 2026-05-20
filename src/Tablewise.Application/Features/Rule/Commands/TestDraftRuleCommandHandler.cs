using MediatR;
using Tablewise.Application.DTOs.Rule;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Exceptions;
using Tablewise.Domain.Interfaces;

namespace Tablewise.Application.Features.Rule.Commands;

/// <summary>
/// Taslak kural test komutu handler'ı.
/// </summary>
public sealed class TestDraftRuleCommandHandler : IRequestHandler<TestDraftRuleCommand, RuleTestResultDto>
{
    private readonly IRuleTestService _ruleTestService;
    private readonly ICurrentUser _currentUser;

    public TestDraftRuleCommandHandler(
        IRuleTestService ruleTestService,
        ICurrentUser currentUser)
    {
        _ruleTestService = ruleTestService;
        _currentUser = currentUser;
    }

    public async Task<RuleTestResultDto> Handle(
        TestDraftRuleCommand request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.Role != Domain.Enums.UserRole.Owner)
        {
            throw new ForbiddenException("Sadece Owner rolüne sahip kullanıcılar kural test edebilir.");
        }

        return await _ruleTestService.TestDraftRuleAsync(request.Dto, cancellationToken)
            .ConfigureAwait(false);
    }
}
