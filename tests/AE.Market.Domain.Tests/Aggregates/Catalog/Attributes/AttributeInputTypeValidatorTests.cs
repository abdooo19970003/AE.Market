using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Attributes;

public sealed class AttributeInputTypeValidatorTests
{
    [Fact]
    public void Validate_TextType_WithTextValue_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Text, "hello", null, null, null, null, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_TextType_WithIntegerValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Text, null, 42, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_TextType_WithBooleanValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Text, null, null, null, true, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_IntegerType_WithIntegerValue_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Integer, null, 42, null, null, null, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_IntegerType_WithTextValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Integer, "hello", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_DecimalType_WithDecimalValue_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Decimal, null, null, 3.14m, null, null, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_DecimalType_WithTextValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Decimal, "hello", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_BooleanType_WithBooleanValue_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Boolean, null, null, null, true, null, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_BooleanType_WithTextValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.Boolean, "hello", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_DateTimeType_WithDateTimeValue_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.DateTime, null, null, null, null, DateTime.UtcNow, null);

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_DateTimeType_WithTextValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.DateTime, "hello", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_MultiSelectType_WithOptionId_Succeeds()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.MultiSelect, null, null, null, null, null, Guid.NewGuid());

        act.Should().NotThrow();
    }

    [Fact]
    public void Validate_MultiSelectType_WithTextValue_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.MultiSelect, "hello", null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Validate_MultiSelectType_WithNullOptionId_Throws()
    {
        var act = () => AttributeInputTypeValidator.Validate(
            AttributeInputType.MultiSelect, null, null, null, null, null, null);

        act.Should().Throw<DomainException>();
    }
}
