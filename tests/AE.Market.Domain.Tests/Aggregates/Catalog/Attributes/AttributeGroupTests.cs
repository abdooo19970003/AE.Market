using AE.Market.Domain.Aggregates.Catalog.Attributes;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Attributes;

public sealed class AttributeGroupTests
{
    [Fact]
    public void Create_WithValidData_ReturnsGroup()
    {
        var id = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var group = AttributeGroup.Create(id, categoryId, "Physical", slug: "physical", sortOrder: 3);

        group.Id.Should().Be(id);
        group.CategoryId.Should().Be(categoryId);
        group.GroupName.Should().Be("Physical");
        group.Slug!.Value.Should().Be("physical");
        group.SortOrder.Should().Be(3);
    }

    [Fact]
    public void Rename_UpdatesProperties()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");

        group.Rename("Dimensions", slug: "dimensions", sortOrder: 7);

        group.GroupName.Should().Be("Dimensions");
        group.Slug!.Value.Should().Be("dimensions");
        group.SortOrder.Should().Be(7);
    }

    [Fact]
    public void AddAttribute_AddsIdToCollection()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");
        var attributeId = Guid.NewGuid();

        group.AddAttribute(attributeId);

        group.AttributeIds.Should().ContainSingle().Which.Should().Be(attributeId);
    }

    [Fact]
    public void AddAttribute_DuplicateId_DoesNotAdd()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");
        var attributeId = Guid.NewGuid();
        group.AddAttribute(attributeId);

        group.AddAttribute(attributeId);

        group.AttributeIds.Should().HaveCount(1);
    }

    [Fact]
    public void RemoveAttribute_RemovesIdFromCollection()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");
        var attributeId = Guid.NewGuid();
        group.AddAttribute(attributeId);

        group.RemoveAttribute(attributeId);

        group.AttributeIds.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAttribute_NonExistentId_DoesNothing()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");

        var act = () => group.RemoveAttribute(Guid.NewGuid());

        act.Should().NotThrow();
        group.AttributeIds.Should().BeEmpty();
    }

    [Fact]
    public void Delete_ClearsAttributeIds()
    {
        var group = AttributeGroup.Create(Guid.NewGuid(), Guid.NewGuid(), "Physical");
        group.AddAttribute(Guid.NewGuid());
        group.AddAttribute(Guid.NewGuid());

        group.Delete();

        group.IsDeleted.Should().BeTrue();
        group.AttributeIds.Should().BeEmpty();
    }
}
