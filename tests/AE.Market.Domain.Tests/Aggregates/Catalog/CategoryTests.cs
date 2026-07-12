using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class CategoryTests
{
    private static Category CreateValidCategory(Guid? parentId = null, string? parentPath = null)
    {
        return Category.Create(
            Guid.NewGuid(),
            "Electronics",
            "electronics",
            "All electronics",
            parentId,
            parentPath: parentPath
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
            var parent = CreateValidCategory();
            var category = CreateValidCategory(parent.Id, parent.Path);

            category.ParentId.Should().Be(parent.Id);
            category.Path.Should().Be($"{parent.Path}{category.Id}/");
        }

        [Fact]
        public void Create_RaisesCategoryCreatedDomainEvent()
        {
            var category = CreateValidCategory();

            category.DomainEvents.Should().ContainSingle(e => e is CategoryCreatedDomainEvent)
                .Which.As<CategoryCreatedDomainEvent>().CategoryId.Should().Be(category.Id);
        }
    }

    public sealed class Path
    {
        [Fact]
        public void Path_ForRootCategory_SetsCorrectPath()
        {
            var id = Guid.NewGuid();
            var category = Category.Create(id, "Root", "root");

            category.Path.Should().Be($"{id}/");
        }

        [Fact]
        public void Path_ForChildCategory_SetsCorrectPath()
        {
            var parent = CreateValidCategory();
            var childId = Guid.NewGuid();
            var child = Category.Create(childId, "Child", "child", null, parent.Id, parentPath: parent.Path);

            child.Path.Should().Be($"{parent.Path}{childId}/");
        }

        [Fact]
        public void Path_ForGrandchildCategory_SetsCorrectPath()
        {
            var parent = CreateValidCategory();
            var child = Category.Create(Guid.NewGuid(), "Child", "child", null, parent.Id, parentPath: parent.Path);
            var grandchildId = Guid.NewGuid();
            var grandchild = Category.Create(grandchildId, "Grandchild", "grandchild", null, child.Id, parentPath: child.Path);

            grandchild.Path.Should().Be($"{child.Path}{grandchildId}/");
        }

        [Fact]
        public void Path_WithParentIdAndNoParentPath_FallsBackToRootPath()
        {
            var category = CreateValidCategory(Guid.NewGuid());

            category.Path.Should().Be($"{category.Id}/");
        }

        [Fact]
        public void Path_AfterChangeParent_UpdatesCorrectly()
        {
            var oldParent = CreateValidCategory();
            var category = CreateValidCategory(oldParent.Id, oldParent.Path);
            var newParent = CreateValidCategory();
            var oldPath = category.Path;

            category.ChangeParent(newParent.Id, newParent.Path);

            category.Path.Should().Be($"{newParent.Path}{category.Id}/");
            category.Path.Should().NotBe(oldPath);
        }

        [Fact]
        public void UpdatePath_SetsNewPath()
        {
            var category = CreateValidCategory();
            var newPath = "custom-path/";

            category.UpdatePath(newPath);

            category.Path.Should().Be(newPath);
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

            category.ChangeParent(newParentId, "new-parent-path/");

            category.ParentId.Should().Be(newParentId);
            category.Path.Should().Be("new-parent-path/" + category.Id + "/");
        }

        [Fact]
        public void ChangeParent_ToNull_ClearsParent()
        {
            var parent = CreateValidCategory();
            var category = CreateValidCategory(parent.Id, parent.Path);

            category.ChangeParent(null);

            category.ParentId.Should().BeNull();
            category.Path.Should().Be(category.Id + "/");
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
            var child = Category.Create(Guid.NewGuid(), "Child", "child", null, parent.Id, parentPath: parent.Path);
            var grandchild = Category.Create(Guid.NewGuid(), "Grandchild", "grandchild", null, child.Id, parentPath: child.Path);

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
            var oldPath = category.Path;

            category.ChangeParent(Guid.NewGuid(), "new-parent-path/");

            var evt = category.DomainEvents.Should().ContainSingle(e => e is CategoryParentChangedDomainEvent)
                .Which.As<CategoryParentChangedDomainEvent>();
            evt.CategoryId.Should().Be(category.Id);
            evt.OldPath.Should().Be(oldPath);
            evt.NewPath.Should().Be("new-parent-path/" + category.Id + "/");
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

        [Fact]
        public void Delete_CascadesToAttributeGroups()
        {
            var category = CreateValidCategory();
            var group = category.AddAttributeGroup(Guid.NewGuid(), "Colors", "colors");

            category.Delete();

            group.IsDeleted.Should().BeTrue();
        }
    }

    public sealed class MetaFields
    {
        [Fact]
        public void SetOrUpdateMetaFields_SetsValues()
        {
            var category = CreateValidCategory();

            category.SetOrUpdateMetaFields("title", "desc", "keywords");

            category.MetaTitle.Should().Be("title");
            category.MetaDescription.Should().Be("desc");
            category.MetaKeywords.Should().Be("keywords");
        }

        [Fact]
        public void SetOrUpdateMetaFields_NullValues_ClearsFields()
        {
            var category = CreateValidCategory();
            category.SetOrUpdateMetaFields("title", "desc", "keywords");

            category.SetOrUpdateMetaFields(null, null, null);

            category.MetaTitle.Should().BeNull();
            category.MetaDescription.Should().BeNull();
            category.MetaKeywords.Should().BeNull();
        }
    }

    public sealed class SlugUpdate
    {
        [Fact]
        public void UpdateSlug_UpdatesSlug()
        {
            var category = CreateValidCategory();

            category.UpdateSlug("new-slug");

            category.Slug.Value.Should().Be("new-slug");
        }

        [Fact]
        public void UpdateSlug_RaisesCategorySlugChangedDomainEvent()
        {
            var category = CreateValidCategory();

            category.UpdateSlug("new-slug");

            category.DomainEvents.Should().Contain(e => e is CategorySlugChangedDomainEvent);
        }
    }

    public sealed class AttributeGroups
    {
        [Fact]
        public void AddAttributeGroup_AddsToCollection()
        {
            var category = CreateValidCategory();

            var group = category.AddAttributeGroup(Guid.NewGuid(), "Colors", "colors", 1);

            category.AttributeGroups.Should().ContainSingle().Which.Should().Be(group);
            group.GroupName.Should().Be("Colors");
        }

        [Fact]
        public void AddAttributeGroup_RaisesAttributeGroupCreatedDomainEvent()
        {
            var category = CreateValidCategory();

            category.AddAttributeGroup(Guid.NewGuid(), "Colors");

            category.DomainEvents.Should().Contain(e => e is AttributeGroupCreatedDomainEvent);
        }

        [Fact]
        public void RemoveAttributeGroup_RemovesFromCollection()
        {
            var category = CreateValidCategory();
            var group = category.AddAttributeGroup(Guid.NewGuid(), "Colors", "colors");

            category.RemoveAttributeGroup(group);

            category.AttributeGroups.Should().BeEmpty();
            group.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void RemoveAttributeGroup_UnassignsAttributesFromGroup()
        {
            var category = CreateValidCategory();
            var group = category.AddAttributeGroup(Guid.NewGuid(), "Colors", "colors");
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, category.Id, true, true);
            category.AddAttribute(attr);
            group.AddAttribute(attr.Id);

            category.RemoveAttributeGroup(group);

            attr.AttributeGroupId.Should().BeNull();
        }
    }

    public sealed class GetEffectiveAttributes
    {
        [Fact]
        public void GetEffectiveAttributes_NoParent_ReturnsOnlyOwn()
        {
            var category = CreateValidCategory();
            var attr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, category.Id, true, true);
            category.AddAttribute(attr);

            var result = category.GetEffectiveAttributes();

            result.Should().ContainSingle();
            result.First().Attribute.Id.Should().Be(attr.Id);
            result.First().IsInherited.Should().BeFalse();
        }

        [Fact]
        public void GetEffectiveAttributes_ChildInheritsAncestorAttributes()
        {
            var parent = CreateValidCategory();
            var parentAttr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, parent.Id, true, true);
            parent.AddAttribute(parentAttr);

            var child = Category.Create(Guid.NewGuid(), "Phones", "phones", null, parent.Id, parentPath: parent.Path);

            typeof(Category).GetProperty("Parent")!
                .SetValue(child, parent);

            var result = child.GetEffectiveAttributes();

            result.Should().ContainSingle();
            result.First().Attribute.Id.Should().Be(parentAttr.Id);
            result.First().IsInherited.Should().BeTrue();
        }

        [Fact]
        public void GetEffectiveAttributes_ChildOwnAttributes_NotInherited()
        {
            var parent = CreateValidCategory();
            var parentAttr = CategoryAttribute.Create(Guid.NewGuid(), "Color", AttributeInputType.Text, parent.Id, true, true);
            parent.AddAttribute(parentAttr);

            var child = Category.Create(Guid.NewGuid(), "Phones", "phones", null, parent.Id, parentPath: parent.Path);
            typeof(Category).GetProperty("Parent")!
                .SetValue(child, parent);

            var childAttr = CategoryAttribute.Create(Guid.NewGuid(), "Size", AttributeInputType.Text, child.Id, true, true);
            child.AddAttribute(childAttr);

            var result = child.GetEffectiveAttributes();

            result.Should().HaveCount(2);
            result.First(a => a.Attribute.Id == parentAttr.Id).IsInherited.Should().BeTrue();
            result.First(a => a.Attribute.Id == childAttr.Id).IsInherited.Should().BeFalse();
        }
    }
}
