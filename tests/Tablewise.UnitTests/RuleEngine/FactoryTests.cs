using Moq;
using Tablewise.Domain.Entities;
using Tablewise.RuleEngine.Facts;
using Tablewise.RuleEngine.Interfaces;
using Tablewise.RuleEngine.Results;
using Tablewise.RuleEngine.Services;

namespace Tablewise.UnitTests.RuleEngine;

/// <summary>
/// RuleTypeEvaluatorFactory birim testleri.
/// </summary>
public class FactoryTests
{
    [Fact]
    public void GetFor_WithKnownRuleType_ReturnsEvaluator()
    {
        // Arrange
        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("EarlyBooking");

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });

        // Act
        var result = factory.GetFor("EarlyBooking");

        // Assert
        Assert.NotNull(result);
        Assert.Same(evaluator.Object, result);
    }

    [Fact]
    public void GetFor_WithUnknownRuleType_ReturnsNull()
    {
        // Arrange
        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("EarlyBooking");

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });

        // Act
        var result = factory.GetFor("UnknownType");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFor_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("EarlyBooking");

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });

        // Act
        var result = factory.GetFor("");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFor_WithNull_ReturnsNull()
    {
        // Arrange
        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("EarlyBooking");

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });

        // Act
        var result = factory.GetFor(null!);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetFor_IsCaseInsensitive()
    {
        // Arrange
        var evaluator = new Mock<IRuleTypeEvaluator>();
        evaluator.Setup(x => x.RuleType).Returns("EarlyBooking");

        var factory = new RuleTypeEvaluatorFactory(new[] { evaluator.Object });

        // Act & Assert
        Assert.NotNull(factory.GetFor("earlybooking"));
        Assert.NotNull(factory.GetFor("EARLYBOOKING"));
        Assert.NotNull(factory.GetFor("earlyBooking"));
    }

    [Fact]
    public void GetFor_WithMultipleEvaluators_ReturnsCorrectOne()
    {
        // Arrange
        var evaluator1 = new Mock<IRuleTypeEvaluator>();
        evaluator1.Setup(x => x.RuleType).Returns("EarlyBooking");

        var evaluator2 = new Mock<IRuleTypeEvaluator>();
        evaluator2.Setup(x => x.RuleType).Returns("VIPPriority");

        var evaluator3 = new Mock<IRuleTypeEvaluator>();
        evaluator3.Setup(x => x.RuleType).Returns("LargeGroup");

        var factory = new RuleTypeEvaluatorFactory(new[]
        {
            evaluator1.Object,
            evaluator2.Object,
            evaluator3.Object
        });

        // Act & Assert
        Assert.Same(evaluator1.Object, factory.GetFor("EarlyBooking"));
        Assert.Same(evaluator2.Object, factory.GetFor("VIPPriority"));
        Assert.Same(evaluator3.Object, factory.GetFor("LargeGroup"));
    }

    [Fact]
    public void Constructor_WithDuplicateRuleTypes_LastOneWins()
    {
        // Arrange
        var evaluator1 = new Mock<IRuleTypeEvaluator>();
        evaluator1.Setup(x => x.RuleType).Returns("EarlyBooking");

        var evaluator2 = new Mock<IRuleTypeEvaluator>();
        evaluator2.Setup(x => x.RuleType).Returns("EarlyBooking"); // Aynı tip

        var factory = new RuleTypeEvaluatorFactory(new[]
        {
            evaluator1.Object,
            evaluator2.Object
        });

        // Act
        var result = factory.GetFor("EarlyBooking");

        // Assert - Son eklenen kazanır
        Assert.Same(evaluator2.Object, result);
    }

    [Fact]
    public void Constructor_WithEmptyList_DoesNotThrow()
    {
        // Arrange & Act
        var factory = new RuleTypeEvaluatorFactory(Enumerable.Empty<IRuleTypeEvaluator>());

        // Assert
        Assert.Null(factory.GetFor("AnyType"));
    }
}
