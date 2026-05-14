using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Tablewise.Application.Interfaces;
using Tablewise.Domain.Entities;
using Tablewise.Domain.Enums;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;
using Tablewise.RuleEngine.Results;
using Tablewise.RuleEngine.Services;

namespace Tablewise.UnitTests.RuleEngine;

/// <summary>
/// RuleEnginePipeline birim testleri.
/// </summary>
public class PipelineTests
{
    private readonly Mock<IApplicationDbContext> _dbContextMock;
    private readonly Mock<ICacheService> _cacheServiceMock;
    private readonly Mock<ILogger<RuleEnginePipeline>> _loggerMock;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _venueId = Guid.NewGuid();

    public PipelineTests()
    {
        _dbContextMock = new Mock<IApplicationDbContext>();
        _cacheServiceMock = new Mock<ICacheService>();
        _loggerMock = new Mock<ILogger<RuleEnginePipeline>>();
    }

    #region Test Helpers

    private ReservationContext CreateContext(int partySize = 4)
    {
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "Test Tenant",
            Slug = "test",
            Email = "test@test.com"
        };

        var venue = new Venue
        {
            Id = _venueId,
            TenantId = _tenantId,
            Name = "Test Venue",
            OpeningTime = TimeSpan.FromHours(10),
            ClosingTime = TimeSpan.FromHours(22)
        };

        var reservation = new Reservation
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            VenueId = _venueId,
            PartySize = partySize,
            ReservedFor = DateTime.UtcNow.AddDays(3),
            GuestName = "Test Guest",
            GuestPhone = "5551234567"
        };

        return new ReservationContext
        {
            Tenant = tenant,
            Venue = venue,
            Reservation = reservation,
            CurrentOccupancyRate = 0.5,
            DaysInAdvance = 3
        };
    }

    private RuleEnginePipeline CreatePipeline(IRuleTypeEvaluatorFactory factory)
    {
        return new RuleEnginePipeline(
            _dbContextMock.Object,
            _cacheServiceMock.Object,
            factory,
            _loggerMock.Object);
    }

    private void SetupDbRules(params Rule[] rules)
    {
        var rulesList = rules.ToList().AsQueryable();
        var mockSet = new Mock<DbSet<Rule>>();

        mockSet.As<IAsyncEnumerable<Rule>>()
            .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
            .Returns(new TestAsyncEnumerator<Rule>(rulesList.GetEnumerator()));

        mockSet.As<IQueryable<Rule>>()
            .Setup(m => m.Provider)
            .Returns(new TestAsyncQueryProvider<Rule>(rulesList.Provider));

        mockSet.As<IQueryable<Rule>>()
            .Setup(m => m.Expression)
            .Returns(rulesList.Expression);

        mockSet.As<IQueryable<Rule>>()
            .Setup(m => m.ElementType)
            .Returns(rulesList.ElementType);

        mockSet.As<IQueryable<Rule>>()
            .Setup(m => m.GetEnumerator())
            .Returns(rulesList.GetEnumerator());

        _dbContextMock.Setup(x => x.Rules).Returns(mockSet.Object);
    }

    private Rule CreateRule(
        string ruleType,
        int priority = 100,
        bool isActive = true,
        Guid? venueId = null)
    {
        return new Rule
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            VenueId = venueId ?? _venueId,
            Name = $"Test Rule - {ruleType}",
            RuleType = ruleType,
            Priority = priority,
            IsActive = isActive,
            ConditionsJson = """{"version":1}""",
            ActionsJson = """{"version":1}"""
        };
    }

    #endregion

    #region Block Outcome Tests

    [Fact]
    public async Task ExecuteAsync_WhenBlockOutcome_StopsPipelineAndSetsIsBlocked()
    {
        // Arrange
        var blockRule = CreateRule("BlockRule", priority: 1);
        var allowRule = CreateRule("AllowRule", priority: 2);

        SetupDbRules(blockRule, allowRule);

        var blockEvaluator = new Mock<IRuleTypeEvaluator>();
        blockEvaluator.Setup(x => x.RuleType).Returns("BlockRule");
        blockEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = blockRule.Id,
                RuleName = blockRule.Name,
                RuleType = blockRule.RuleType,
                ActionType = RuleActionType.Block,
                Message = "Blocked by test"
            });

        var allowEvaluator = new Mock<IRuleTypeEvaluator>();
        allowEvaluator.Setup(x => x.RuleType).Returns("AllowRule");
        allowEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = allowRule.Id,
                RuleName = allowRule.Name,
                RuleType = allowRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { blockEvaluator.Object, allowEvaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.True(result.IsBlocked);
        Assert.Equal("Blocked by test", result.BlockReason);
        Assert.Single(result.Outcomes); // Sadece block outcome, allow çalışmadı

        // AllowRule evaluator'ı çağrılmadı
        allowEvaluator.Verify(
            x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Null Outcome Tests

    [Fact]
    public async Task ExecuteAsync_WhenNullOutcome_SkipsAndContinues()
    {
        // Arrange
        var skipRule = CreateRule("SkipRule", priority: 1);
        var allowRule = CreateRule("AllowRule", priority: 2);

        SetupDbRules(skipRule, allowRule);

        var skipEvaluator = new Mock<IRuleTypeEvaluator>();
        skipEvaluator.Setup(x => x.RuleType).Returns("SkipRule");
        skipEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RuleOutcome?)null);

        var allowEvaluator = new Mock<IRuleTypeEvaluator>();
        allowEvaluator.Setup(x => x.RuleType).Returns("AllowRule");
        allowEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = allowRule.Id,
                RuleName = allowRule.Name,
                RuleType = allowRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { skipEvaluator.Object, allowEvaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.False(result.IsBlocked);
        Assert.Single(result.Outcomes); // Sadece allow outcome

        // Her iki evaluator da çağrıldı
        skipEvaluator.Verify(
            x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()),
            Times.Once);
        allowEvaluator.Verify(
            x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Priority Order Tests

    [Fact]
    public async Task ExecuteAsync_RulesExecuteInPriorityOrder()
    {
        // Arrange
        var highPriorityRule = CreateRule("HighPriority", priority: 1);
        var mediumPriorityRule = CreateRule("MediumPriority", priority: 50);
        var lowPriorityRule = CreateRule("LowPriority", priority: 100);

        // DB'den priority sıralı gelecekler
        SetupDbRules(highPriorityRule, mediumPriorityRule, lowPriorityRule);

        var executionOrder = new List<string>();

        var highEvaluator = new Mock<IRuleTypeEvaluator>();
        highEvaluator.Setup(x => x.RuleType).Returns("HighPriority");
        highEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .Callback(() => executionOrder.Add("HighPriority"))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = highPriorityRule.Id,
                RuleName = highPriorityRule.Name,
                RuleType = highPriorityRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var mediumEvaluator = new Mock<IRuleTypeEvaluator>();
        mediumEvaluator.Setup(x => x.RuleType).Returns("MediumPriority");
        mediumEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .Callback(() => executionOrder.Add("MediumPriority"))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = mediumPriorityRule.Id,
                RuleName = mediumPriorityRule.Name,
                RuleType = mediumPriorityRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var lowEvaluator = new Mock<IRuleTypeEvaluator>();
        lowEvaluator.Setup(x => x.RuleType).Returns("LowPriority");
        lowEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .Callback(() => executionOrder.Add("LowPriority"))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = lowPriorityRule.Id,
                RuleName = lowPriorityRule.Name,
                RuleType = lowPriorityRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { highEvaluator.Object, mediumEvaluator.Object, lowEvaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert
        Assert.Equal(new[] { "HighPriority", "MediumPriority", "LowPriority" }, executionOrder);
    }

    #endregion

    #region Unknown RuleType Tests

    [Fact]
    public async Task ExecuteAsync_WhenUnknownRuleType_SkipsWithoutException()
    {
        // Arrange
        var unknownRule = CreateRule("UnknownType", priority: 1);
        var knownRule = CreateRule("KnownType", priority: 2);

        SetupDbRules(unknownRule, knownRule);

        var knownEvaluator = new Mock<IRuleTypeEvaluator>();
        knownEvaluator.Setup(x => x.RuleType).Returns("KnownType");
        knownEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = knownRule.Id,
                RuleName = knownRule.Name,
                RuleType = knownRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        // Sadece KnownType için evaluator var
        var factory = new RuleTypeEvaluatorFactory(new[] { knownEvaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act - Exception atmamalı
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.False(result.IsBlocked);
        Assert.Single(result.Outcomes); // Sadece known rule outcome'u
    }

    #endregion

    #region Tenant-Wide Rule Tests

    [Fact]
    public async Task ExecuteAsync_TenantWideRule_AppliesWhenVenueIdNull()
    {
        // Arrange
        var tenantWideRule = CreateRule("TenantWide", priority: 1, venueId: null);
        tenantWideRule.VenueId = null; // Tenant-wide kural

        var venueSpecificRule = CreateRule("VenueSpecific", priority: 2, venueId: _venueId);

        SetupDbRules(tenantWideRule, venueSpecificRule);

        var tenantWideEvaluator = new Mock<IRuleTypeEvaluator>();
        tenantWideEvaluator.Setup(x => x.RuleType).Returns("TenantWide");
        tenantWideEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = tenantWideRule.Id,
                RuleName = tenantWideRule.Name,
                RuleType = tenantWideRule.RuleType,
                ActionType = RuleActionType.Warn,
                Message = "Tenant-wide warning"
            });

        var venueEvaluator = new Mock<IRuleTypeEvaluator>();
        venueEvaluator.Setup(x => x.RuleType).Returns("VenueSpecific");
        venueEvaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = venueSpecificRule.Id,
                RuleName = venueSpecificRule.Name,
                RuleType = venueSpecificRule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { tenantWideEvaluator.Object, venueEvaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.Equal(2, result.Outcomes.Count);
        Assert.Single(result.Warnings);
        Assert.Contains("Tenant-wide warning", result.Warnings);
    }

    #endregion

    #region Discount Aggregation Tests

    [Fact]
    public async Task ExecuteAsync_MultipleDiscounts_AggregatesTotal()
    {
        // Arrange
        var discount1 = CreateRule("Discount1", priority: 1);
        var discount2 = CreateRule("Discount2", priority: 2);

        SetupDbRules(discount1, discount2);

        var evaluator1 = new Mock<IRuleTypeEvaluator>();
        evaluator1.Setup(x => x.RuleType).Returns("Discount1");
        evaluator1
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = discount1.Id,
                RuleName = discount1.Name,
                RuleType = discount1.RuleType,
                ActionType = RuleActionType.Discount,
                Payload = """{"discountPercent": 10}"""
            });

        var evaluator2 = new Mock<IRuleTypeEvaluator>();
        evaluator2.Setup(x => x.RuleType).Returns("Discount2");
        evaluator2
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = discount2.Id,
                RuleName = discount2.Name,
                RuleType = discount2.RuleType,
                ActionType = RuleActionType.Discount,
                Payload = """{"discountPercent": 5}"""
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator1.Object, evaluator2.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.Equal(15m, result.TotalDiscountPercent);
    }

    #endregion

    #region Deposit Tests

    [Fact]
    public async Task ExecuteAsync_DepositOutcome_SetsRequiresDepositAndAmount()
    {
        // Arrange
        var depositRule = CreateRule("Deposit", priority: 1);

        SetupDbRules(depositRule);

        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("Deposit");
        evaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = depositRule.Id,
                RuleName = depositRule.Name,
                RuleType = depositRule.RuleType,
                ActionType = RuleActionType.Deposit,
                Payload = """{"depositAmount": 100}"""
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        var result = await pipeline.ExecuteAsync(context);

        // Assert
        Assert.True(result.RequiresDeposit);
        Assert.Equal(100m, result.DepositAmount);
    }

    #endregion

    #region Cache Tests

    [Fact]
    public async Task ExecuteAsync_WhenCacheMiss_QueriesDb()
    {
        // Arrange
        var rule = CreateRule("TestType", priority: 1);
        SetupDbRules(rule);

        // Cache miss - null döndür (generic yok, herhangi bir key için null)
        // Cache servisi hiçbir şey yapmasın, DB'ye gidilecek

        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("TestType");
        evaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleType = rule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert - DB'ye gidildi
        _dbContextMock.Verify(x => x.Rules, Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenRulesFound_WritesToCache()
    {
        // Arrange
        var rule = CreateRule("TestType", priority: 1);
        SetupDbRules(rule);

        // Cache servisi hiçbir şey yapmasın, DB'ye gidilecek ve cache'e yazılacak

        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("TestType");
        evaluator
            .Setup(x => x.EvaluateAsync(It.IsAny<Rule>(), It.IsAny<ReservationContext>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RuleOutcome
            {
                RuleId = rule.Id,
                RuleName = rule.Name,
                RuleType = rule.RuleType,
                ActionType = RuleActionType.Allow
            });

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });
        var pipeline = CreatePipeline(factory);
        var context = CreateContext();

        // Act
        await pipeline.ExecuteAsync(context);

        // Assert - Cache'e yazıldı
        _cacheServiceMock.Verify(
            x => x.SetAsync(
                It.Is<string>(k => k.Contains("rules:")),
                It.IsAny<object>(),
                It.Is<TimeSpan?>(t => t.HasValue && t.Value.TotalMinutes == 5),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}

#region Test Async Query Helpers

/// <summary>
/// In-memory async enumerable testi için helper.
/// </summary>
internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}

/// <summary>
/// In-memory async queryable testi için helper.
/// </summary>
internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments()[0];
        var executeMethod = typeof(IQueryProvider)
            .GetMethods()
            .First(m => m.Name == nameof(IQueryProvider.Execute) && m.IsGenericMethod)
            .MakeGenericMethod(resultType);

        var result = executeMethod.Invoke(_inner, new object[] { expression });
        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(resultType)
            .Invoke(null, new[] { result })!;
    }
}

/// <summary>
/// In-memory async enumerable testi için helper.
/// </summary>
internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
    { }

    public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
}

#endregion
