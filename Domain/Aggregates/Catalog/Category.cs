using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog;

public sealed class Category : BaseEntity, IAggregateRoot, IMetaData
{
    public string CategoryName { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public Slug Slug { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int SortOrder { get; private set; }

    // The Materialized Path (e.g., "00000000-0000-.../guid-guid-guid/")
    public string Path { get; private set; } = string.Empty;
    public URL CategoryUrl => URL.Create("Categories", Slug);

    public Guid? ParentId { get; private set; }
    public Category? Parent { get; private set; }

    private readonly List<Category> _subCategories = [];
    public IReadOnlyCollection<Category> SubCategories => _subCategories.AsReadOnly();

    private readonly List<CategoryAttribute> _attributes = [];
    public IReadOnlyCollection<CategoryAttribute> Attributes => _attributes.AsReadOnly();

    private readonly List<AttributeGroup> _attributeGroups = [];
    public IReadOnlyCollection<AttributeGroup> AttributeGroups => _attributeGroups.AsReadOnly();

    // SEO
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public string? MetaKeywords { get; private set; }

    private Category(
        Guid id,
        string name,
        Slug slug,
        string? description,
        Guid? parentId,
        string? imageUrl,
        int sortOrder,
        string? parentPath
    )
        : base(id)
    {
        CategoryName = name;
        Slug = slug;
        Description = description ?? string.Empty;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;

        if (parentId is not null)
        {
            if (parentId == id)
                throw new DomainException(CatalogErrors.CategoryCannotBeOwnChild.Code, CatalogErrors.CategoryCannotBeOwnChild.Message);
            ParentId = parentId;
            Path = parentPath is not null ? $"{parentPath}{id}/" : $"{id}/";
        }
        else
        {
            ParentId = null;
            Path = $"{id}/";
        }
    }

    private Category() { }

    public static Category Create(
        Guid id,
        string name,
        string slug,
        string? description = null,
        Guid? parentId = null,
        string? imageUrl = null,
        int sortOrder = 0,
        string? parentPath = null
    )
    {
        var category = new Category(id, name, Slug.Create(slug), description, parentId, imageUrl, sortOrder, parentPath);
        category.AddDomainEvent(new CategoryCreatedDomainEvent(category.Id));
        return category;
    }

    public void UpdateDetails(string name, string? description, string? imageUrl, int sortOrder)
    {
        CategoryName = name;
        Description = description ?? string.Empty;
        ImageUrl = imageUrl;
        SortOrder = sortOrder;
        AddDomainEvent(new CategoryDetailsUpdatedDomainEvent(Id, name, Description, imageUrl, sortOrder));
        UpdateLastModified();
    }

    public void UpdateSlug(string slug)
    {
        var oldSlug = Slug;
        Slug = Slug.Create(slug);
        AddDomainEvent(new CategorySlugChangedDomainEvent(Id, oldSlug.Value, slug));
        UpdateLastModified();
    }

    public void Activate()
    {
        if (IsActive)
            return;
        IsActive = true;
        AddDomainEvent(new CategoryActivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public void Deactivate()
    {
        if (!IsActive)
            return;
        IsActive = false;
        AddDomainEvent(new CategoryDeactivatedDomainEvent(Id));
        UpdateLastModified();
    }

    public void ChangeParent(Guid? newParentId, string? newParentPath = null)
    {
        if (newParentId == Id)
            throw new DomainException(CatalogErrors.CategoryCannotBeOwnChild.Code, CatalogErrors.CategoryCannotBeOwnChild.Message);

        if (newParentId is not null && IsDescendantOf(newParentId.Value))
            throw new DomainException(CatalogErrors.CategoryCannotBeOwnDescendant.Code, CatalogErrors.CategoryCannotBeOwnDescendant.Message);

        if (newParentId == ParentId)
            return;

        var oldParentId = ParentId;
        var oldPath = Path;
        ParentId = newParentId;
        Path = newParentId is not null && newParentPath is not null
            ? $"{newParentPath}{Id}/"
            : $"{Id}/";
        AddDomainEvent(new CategoryParentChangedDomainEvent(Id, oldParentId, newParentId, oldPath, Path));
        UpdateLastModified();
    }

    internal void UpdatePath(string newPath)
    {
        Path = newPath;
        UpdateLastModified();
    }

    private bool IsDescendantOf(Guid categoryId)
    {
        foreach (var sub in _subCategories)
        {
            if (sub.Id == categoryId)
                return true;
            if (sub.IsDescendantOf(categoryId))
                return true;
        }
        return false;
    }

    public IReadOnlyCollection<EffectiveAttribute> GetEffectiveAttributes()
    {
        var map = new Dictionary<Guid, EffectiveAttribute>();
        CollectAncestorAttributes(this, map, isInherited: false);
        return map.Values.OrderBy(a => a.Attribute.SortOrder).ToList().AsReadOnly();
    }

    private static void CollectAncestorAttributes(Category category, Dictionary<Guid, EffectiveAttribute> map, bool isInherited)
    {
        if (category.Parent is not null)
            CollectAncestorAttributes(category.Parent, map, isInherited: true);

        foreach (var attr in category._attributes)
        {
            if (!map.ContainsKey(attr.Id))
                map[attr.Id] = new EffectiveAttribute(attr, isInherited);
        }
    }

    public sealed record EffectiveAttribute(CategoryAttribute Attribute, bool IsInherited);

    private bool HasAttributeOnAncestor(Guid attributeId)
    {
        if (Parent is not null)
        {
            if (Parent._attributes.Any(a => a.Id == attributeId))
                return true;
            return Parent.HasAttributeOnAncestor(attributeId);
        }
        return false;
    }

    public CategoryAttribute AddAttribute(CategoryAttribute attribute)
    {
        if (_attributes.Any(a => a.Id == attribute.Id))
            throw new DomainException(CatalogErrors.AttributeAlreadyDefinedOnParent.Code, CatalogErrors.AttributeAlreadyDefinedOnParent.Message);

        if (HasAttributeOnAncestor(attribute.Id))
            throw new DomainException(CatalogErrors.AttributeAlreadyDefinedOnParent.Code, CatalogErrors.AttributeAlreadyDefinedOnParent.Message);

        _attributes.Add(attribute);
        AddDomainEvent(new CategoryAttributeAddedDomainEvent(Id, attribute.Id, attribute.AttributeName));
        if (attribute.IsRequired)
        {
            AddDomainEvent(new RequiredAttributeAddedToCategoryDomainEvent(Id, attribute.Id, attribute.AttributeName));
        }
        UpdateLastModified();
        return attribute;
    }

    public void RemoveAttribute(CategoryAttribute attribute)
    {
        attribute.Delete();
        _attributes.Remove(attribute);
        AddDomainEvent(new CategoryAttributeRemovedDomainEvent(Id, attribute.Id));
        UpdateLastModified();
    }

    public void SetOrUpdateMetaFields(string? title, string? description, string? keywords)
    {
        MetaTitle = title;
        MetaDescription = description;
        MetaKeywords = keywords;
        AddDomainEvent(new CategoryMetaFieldsUpdatedDomainEvent(Id, title, description, keywords));
        UpdateLastModified();
    }

    public AttributeGroup AddAttributeGroup(Guid groupId, string groupName, string? slug = null, int sortOrder = 0)
    {
        var group = AttributeGroup.Create(groupId, Id, groupName, slug, sortOrder);
        _attributeGroups.Add(group);
        AddDomainEvent(new AttributeGroupCreatedDomainEvent(groupId, Id));
        UpdateLastModified();
        return group;
    }

    public void RemoveAttributeGroup(AttributeGroup group)
    {
        foreach (var attrId in group.AttributeIds)
        {
            var attr = _attributes.FirstOrDefault(a => a.Id == attrId);
            attr?.AssignToGroup(null);
        }
        group.Delete();
        _attributeGroups.Remove(group);
        AddDomainEvent(new AttributeGroupDeletedDomainEvent(group.Id, Id));
        UpdateLastModified();
    }

    public override void Delete()
    {
        foreach (var sub in _subCategories)
            sub.Delete();
        foreach (var attribute in _attributes)
            attribute.Delete();
        foreach (var group in _attributeGroups)
            group.Delete();
        AddDomainEvent(new CategoryDeletedDomainEvent(Id));
        base.Delete();
    }
}
