using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Products;

public sealed class ProductAttributeValueTests
{
    [Fact]
    public void Create_TextType_WithTextValue_SetsValueText()
    {
        var value = ProductAttributeValue.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            AttributeInputType.Text, textValue: "Red");

        value.ValueText.Should().Be("Red");
        value.ValueInteger.Should().BeNull();
    }

    [Fact]
    public void Create_IntegerType_WithIntValue_SetsValueInteger()
    {
        var value = ProductAttributeValue.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            AttributeInputType.Integer, integerValue: 42);

        value.ValueInteger.Should().Be(42);
        value.ValueText.Should().BeNull();
    }

    [Fact]
    public void UpdateValue_ChangesStoredValue()
    {
        var value = ProductAttributeValue.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            AttributeInputType.Text, textValue: "Red");

        value.UpdateValue(AttributeInputType.Text, textValue: "Blue");

        value.ValueText.Should().Be("Blue");
    }
}
