using AE.Market.Domain.Aggregates.Catalog.Attributes;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Attributes;

public sealed class AttributeOptionTests
{
    [Fact]
    public void Create_WithValidData_ReturnsOption()
    {
        var id = Guid.NewGuid();
        var attributeId = Guid.NewGuid();

        var option = AttributeOption.Create(id, attributeId, "Large", "L", sortOrder: 2);

        option.Id.Should().Be(id);
        option.AttributeId.Should().Be(attributeId);
        option.Label.Should().Be("Large");
        option.Value.Should().Be("L");
        option.SortOrder.Should().Be(2);
    }

    [Fact]
    public void Update_ModifiesProperties()
    {
        var option = AttributeOption.Create(Guid.NewGuid(), Guid.NewGuid(), "Large", "L");

        option.Update("Medium", "M", sortOrder: 1);

        option.Label.Should().Be("Medium");
        option.Value.Should().Be("M");
        option.SortOrder.Should().Be(1);
    }
}
