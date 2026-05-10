using Tablewise.Application.Services;

namespace Tablewise.UnitTests.Services;

/// <summary>
/// RuleSchemaValidator birim testleri.
/// </summary>
public class RuleSchemaValidatorTests
{
    [Fact]
    public void ValidateConditions_ValidConditions_ReturnsSuccess()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "operator": "and",
            "conditions": [
                {"field": "partySize", "op": ">=", "value": 6},
                {"field": "daysInAdvance", "op": "<=", "value": 7}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_UnknownField_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "unknownField", "op": "==", "value": "test"}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Bilinmeyen alan", result.ErrorMessage);
        Assert.Contains("unknownField", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_InvalidOperator_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "partySize", "op": "invalidOp", "value": 5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Bilinmeyen operatör", result.ErrorMessage);
        Assert.Contains("invalidOp", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_InvalidRatioValue_TooHigh_ReturnsError()
    {
        // Arrange - femaleRatio > 1.0
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "femaleRatio", "op": ">=", "value": 1.5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("0.0-1.0", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_InvalidRatioValue_Negative_ReturnsError()
    {
        // Arrange - maleRatio < 0
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "maleRatio", "op": "<=", "value": -0.5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("0.0-1.0", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_InvalidGroupComposition_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "groupComposition", "op": "==", "value": "InvalidComposition"}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Geçersiz grup kompozisyonu", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_ValidGroupComposition_ReturnsSuccess()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "groupComposition", "op": "==", "value": "AllMale"}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateConditions_InvalidCustomerTier_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "customer.tier", "op": "==", "value": "SuperVIP"}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Geçersiz müşteri tier", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_ValidCustomerTier_ReturnsSuccess()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "customer.tier", "op": "in", "value": ["Gold", "VIP"]}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateConditions_InvalidDayOfWeek_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "reservation.reservedFor.dayOfWeek", "op": "==", "value": "Pazartesi"}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("Geçersiz gün", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_InvalidHour_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "reservation.reservedFor.hour", "op": "==", "value": 25}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("0-23", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_MissingVersion_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "conditions": [
                {"field": "partySize", "op": ">=", "value": 5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("version", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_MissingFieldInCondition_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"op": ">=", "value": 5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("field", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_MissingOpInCondition_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "partySize", "value": 5}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("op", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_MissingValueInCondition_ReturnsError()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "partySize", "op": ">="}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("value", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_EmptyJson_ReturnsError()
    {
        // Arrange
        var json = "";

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateConditions_InvalidJson_ReturnsError()
    {
        // Arrange
        var json = "{ invalid json }";

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains("JSON", result.ErrorMessage);
    }

    [Fact]
    public void ValidateConditions_NoConditionsArray_ReturnsSuccess()
    {
        // Arrange - conditions array olmadan da geçerli olabilir (diğer kural tipleri için)
        var json = """
        {
            "version": 1,
            "minDaysInAdvance": 7
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateConditions_InOperatorWithValidArray_ReturnsSuccess()
    {
        // Arrange
        var json = """
        {
            "version": 1,
            "conditions": [
                {"field": "reservation.reservedFor.dayOfWeek", "op": "in", "value": ["Friday", "Saturday", "Sunday"]}
            ]
        }
        """;

        // Act
        var result = RuleSchemaValidator.ValidateConditions(json);

        // Assert
        Assert.True(result.IsValid);
    }
}
