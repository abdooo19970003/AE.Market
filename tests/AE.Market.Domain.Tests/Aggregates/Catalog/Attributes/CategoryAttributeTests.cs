using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Attributes;

public sealed class CategoryAttributeTests
{
    [Fact]
    public void Create_WithValidData_ReturnsAttribute()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var attribute = CategoryAttribute.Create(id, "Color", AttributeInputType.Text, categoryId,
            isRequired: true, isFilterable: true, sortOrder: 5);

        attribute.Id.Should().Be(id);
        attribute.AttributeName.Should().Be("Color");
        attribute.InputType.Should().Be(AttributeInputType.Text);
        attribute.CategoryId.Should().Be(categoryId);
        attribute.IsRequired.Should().BeTrue();
        attribute.IsFilterable.Should().BeTrue();
        attribute.SortOrder.Should().Be(5);
    }

    [Fact]
    public void Update_ModifiesProperties()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, Guid.NewGuid());

        attribute.Update("Size", isRequired: true, isFilterable: true, sortOrder: 10);

        attribute.AttributeName.Should().Be("Size");
        attribute.IsRequired.Should().BeTrue();
        attribute.IsFilterable.Should().BeTrue();
        attribute.SortOrder.Should().Be(10);
    }

    [Fact]
    public void AddOption_AddsToCollection()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, Guid.NewGuid());

        var option = attribute.AddOption(Guid.NewGuid(), "Large", "L");

        attribute.Options.Should().ContainSingle().Which.Should().Be(option);
    }

    [Fact]
    public void AddOption_WithDuplicateValue_Throws()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, Guid.NewGuid());
        attribute.AddOption(Guid.NewGuid(), "Large", "L");

        var act = () => attribute.AddOption(Guid.NewGuid(), "Large (Duplicate)", "L");

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveOption_RemovesAndSoftDeletes()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, Guid.NewGuid());
        var option = attribute.AddOption(Guid.NewGuid(), "Large", "L");

        attribute.RemoveOption(option);

        attribute.Options.Should().BeEmpty();
        option.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void AssignToGroup_SetsAttributeGroupId()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, Guid.NewGuid());
        var groupId = Guid.NewGuid();

        attribute.AssignToGroup(groupId);

        attribute.AttributeGroupId.Should().Be(groupId);
    }

    [Fact]
    public void AssignToGroup_Null_ClearsAttributeGroupId()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, Guid.NewGuid());
        attribute.AssignToGroup(Guid.NewGuid());

        attribute.AssignToGroup(null);

        attribute.AttributeGroupId.Should().BeNull();
    }

    [Fact]
    public void Delete_CascadesToOptions()
    {
        var attribute = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, Guid.NewGuid());
        var option1 = attribute.AddOption(Guid.NewGuid(), "Small", "S");
        var option2 = attribute.AddOption(Guid.NewGuid(), "Large", "L");

        attribute.Delete();

        attribute.IsDeleted.Should().BeTrue();
        option1.IsDeleted.Should().BeTrue();
        option2.IsDeleted.Should().BeTrue();
    }
}
