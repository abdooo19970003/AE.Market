using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class CategoryTests
{
    private static Category CreateValidCategory(Guid? parentId = null)
    {
        return Category.Create(
            Guid.NewGuid(),
            "Electronics",
            "electronics",
            "All electronics",
            parentId
        );
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsCategory()
        {
            var id = Guid.NewGuid();
            var category = Category.Create(id, "Electronics", "electronics", "All electronics");

            category.Id.Should().Be(id);
            category.CategoryName.Should().Be("Electronics");
            category.Slug.Value.Should().Be("electronics");
            category.Description.Should().Be("All electronics");
            category.IsActive.Should().BeTrue();
            category.ParentId.Should().BeNull();
        }

        [Fact]
        public void Create_WithParentId_SetsParent()
        {
            var parentId = Guid.NewGuid();
            var category = CreateValidCategory(parentId);

            category.ParentId.Should().Be(parentId);
        }

        [Fact]
        public void Create_RaisesCategoryCreatedDomainEvent()
        {
            var category = CreateValidCategory();

            category.DomainEvents.Should().ContainSingle(e => e is CategoryCreatedDomainEvent)
                .Which.As<CategoryCreatedDomainEvent>().CategoryId.Should().Be(category.Id);
        }
    }

    public sealed class UpdateDetails
    {
        [Fact]
        public void UpdateDetails_UpdatesProperties()
        {
            var category = CreateValidCategory();

            category.UpdateDetails("New Name", "New desc", "image.jpg", 5);

            category.CategoryName.Should().Be("New Name");
            category.Description.Should().Be("New desc");
            category.ImageUrl.Should().Be("image.jpg");
            category.SortOrder.Should().Be(5);
        }

        [Fact]
        public void UpdateDetails_RaisesCategoryDetailsUpdatedDomainEvent()
        {
            var category = CreateValidCategory();

            category.UpdateDetails("New Name", "New desc", null, 0);

            category.DomainEvents.Should().ContainSingle(e => e is CategoryDetailsUpdatedDomainEvent)
                .Which.As<CategoryDetailsUpdatedDomainEvent>().CategoryId.Should().Be(category.Id);
        }
    }

    public sealed class ChangeParent
    {
        [Fact]
        public void ChangeParent_ToDifferentParent_UpdatesParentId()
        {
            var category = CreateValidCategory();
            var newParentId = Guid.NewGuid();

            category.ChangeParent(newParentId);

            category.ParentId.Should().Be(newParentId);
        }

        [Fact]
        public void ChangeParent_ToNull_ClearsParent()
        {
            var parentId = Guid.NewGuid();
            var category = CreateValidCategory(parentId);

            category.ChangeParent(null);

            category.ParentId.Should().BeNull();
        }

        [Fact]
        public void ChangeParent_ToOwnId_Throws()
        {
            var category = CreateValidCategory();

            var act = () => category.ChangeParent(category.Id);

            act.Should().Throw<DomainException>();
        }

        [Fact]
        public void ChangeParent_ToDescendant_Throws()
        {
            var parent = CreateValidCategory();
            var child = Category.Create(Guid.NewGuid(), "Child", "child", null, parent.Id);
            var grandchild = Category.Create(Guid.NewGuid(), "Grandchild", "grandchild", null, child.Id);

            // Simulate the loaded graph: add subcategories to the in-memory collection
            // This mimics what EF would load via navigation properties
            typeof(Category).GetField("_subCategories",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(parent, new List<Category> { child });

            typeof(Category).GetField("_subCategories",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .SetValue(child, new List<Category> { grandchild });

            // Try to set parent as child of grandchild (would create cycle)
            var act = () => parent.ChangeParent(grandchild.Id);

            act.Should().Throw<DomainException>()
                .WithMessage("*descendant*");
        }

        [Fact]
        public void ChangeParent_RaisesCategoryParentChangedDomainEvent()
        {
            var category = CreateValidCategory();

            category.ChangeParent(Guid.NewGuid());

            category.DomainEvents.Should().ContainSingle(e => e is CategoryParentChangedDomainEvent)
                .Which.As<CategoryParentChangedDomainEvent>().CategoryId.Should().Be(category.Id);
        }
    }

    public sealed class ActivateDeactivate
    {
        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var category = CreateValidCategory();

            category.Deactivate();

            category.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_SetsIsActiveTrue()
        {
            var category = CreateValidCategory();
            category.Deactivate();

            category.Activate();

            category.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Deactivate_RaisesCategoryDeactivatedDomainEvent()
        {
            var category = CreateValidCategory();

            category.Deactivate();

            category.DomainEvents.Should().ContainSingle(e => e is CategoryDeactivatedDomainEvent)
                .Which.As<CategoryDeactivatedDomainEvent>().CategoryId.Should().Be(category.Id);
        }
    }

    public sealed class Attributes
    {
        [Fact]
        public void AddAttribute_AddsToCollection()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, category.Id, true, true);

            var result = category.AddAttribute(attr);

            category.Attributes.Should().ContainSingle().Which.Should().Be(attr);
            result.CategoryId.Should().Be(category.Id);
        }

        [Fact]
        public void RemoveAttribute_RemovesAndSoftDeletes()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, category.Id, true, true);
            category.AddAttribute(attr);

            category.RemoveAttribute(attr);

            category.Attributes.Should().BeEmpty();
            attr.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void RemoveAttribute_WithOptions_CascadesSoftDelete()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, category.Id, true, true);
            category.AddAttribute(attr);
            var option = attr.AddOption(Guid.NewGuid(), "Large", "L");

            category.RemoveAttribute(attr);

            attr.IsDeleted.Should().BeTrue();
            option.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class DeleteCascade
    {
        [Fact]
        public void Delete_CascadesToAttributes()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, category.Id, true, true);
            category.AddAttribute(attr);

            category.Delete();

            category.IsDeleted.Should().BeTrue();
            attr.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_CascadesToAttributeOptions()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.MultiSelect, category.Id, true, true);
            category.AddAttribute(attr);
            var option = attr.AddOption(Guid.NewGuid(), "Small", "S");

            category.Delete();

            option.IsDeleted.Should().BeTrue();
        }
    }
}
