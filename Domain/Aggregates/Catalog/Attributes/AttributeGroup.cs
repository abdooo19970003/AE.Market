using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Attributes;

public sealed class AttributeGroup : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public string GroupName { get; private set; }
    public Slug? Slug { get; private set; }
    public int SortOrder { get; private set; }

    private readonly List<Guid> _attributeIds = [];
    public IReadOnlyCollection<Guid> AttributeIds => _attributeIds.AsReadOnly();

    private AttributeGroup(Guid id, Guid categoryId, string groupName, Slug? slug, int sortOrder)
        : base(id)
    {
        CategoryId = categoryId;
        GroupName = groupName;
        Slug = slug;
        SortOrder = sortOrder;
    }

    private AttributeGroup() { }

    internal static AttributeGroup Create(Guid id, Guid categoryId, string groupName, string? slug = null, int sortOrder = 0)
    {
        return new AttributeGroup(id, categoryId, groupName, Slug.CreateNullable(slug), sortOrder);
    }

    internal void Rename(string groupName, string? slug, int sortOrder)
    {
        GroupName = groupName;
        Slug = Slug.CreateNullable(slug);
        SortOrder = sortOrder;
        UpdateLastModified();
    }

    internal void AddAttribute(Guid attributeId)
    {
        if (!_attributeIds.Contains(attributeId))
            _attributeIds.Add(attributeId);
    }

    internal void RemoveAttribute(Guid attributeId)
    {
        _attributeIds.Remove(attributeId);
    }

    public override void Delete()
    {
        _attributeIds.Clear();
        base.Delete();
    }
}
