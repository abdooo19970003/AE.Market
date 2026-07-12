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
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            AttributeInputType.Text, textValue: "Red");

        value.ValueText.Should().Be("Red");
        value.ValueInteger.Should().BeNull();
    }

    [Fact]
    public void Create_IntegerType_WithIntValue_SetsValueInteger()
    {
        var value = ProductAttributeValue.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            AttributeInputType.Integer, integerValue: 42);

        value.ValueInteger.Should().Be(42);
        value.ValueText.Should().BeNull();
    }

    [Fact]
    public void UpdateValue_ChangesStoredValue()
    {
        var value = ProductAttributeValue.Create(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
            AttributeInputType.Text, textValue: "Red");

        value.UpdateValue(AttributeInputType.Text, textValue: "Blue");

        value.ValueText.Should().Be("Blue");
    }

    public sealed class CreateByType
    {
        [Fact]
        public void Create_DecimalType_SetsValueDecimal()
        {
            var value = ProductAttributeValue.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
                AttributeInputType.Decimal, decimalValue: 19.99m);

            value.ValueDecimal.Should().Be(19.99m);
        }

        [Fact]
        public void Create_BooleanType_SetsValueBoolean()
        {
            var value = ProductAttributeValue.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
                AttributeInputType.Boolean, booleanValue: true);

            value.ValueBoolean.Should().BeTrue();
        }

        [Fact]
        public void Create_DateTimeType_SetsValueDateTime()
        {
            var now = DateTime.UtcNow;
            var value = ProductAttributeValue.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
                AttributeInputType.DateTime, dateTimeValue: now);

            value.ValueDateTime.Should().BeCloseTo(now, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_MultiSelectType_SetsValueOptionId()
        {
            var optionId = Guid.NewGuid();
            var value = ProductAttributeValue.Create(
                Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), null, null,
                AttributeInputType.MultiSelect, optionId: optionId);

            value.ValueOptionId.Should().Be(optionId);
        }
    }
}
