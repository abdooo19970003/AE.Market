using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Catalog.Attributes;

public sealed class CategoryAttribute : BaseEntity
{
    public string AttributeName { get; private set; }
    public Slug? Slug { get; private set; }
    public AttributeInputType InputType { get; private set; }

    public bool IsRequired { get; private set; }
    public bool IsFilterable { get; private set; }
    public int SortOrder { get; private set; }

    public Guid? DefaultUnitId { get; private set; }

    public Guid? AllowedGroupUnitId { get; private set; }

    public Guid CategoryId { get; private set; }
    public Guid? AttributeGroupId { get; private set; }

    private readonly List<AttributeOption> _options = [];
    public IReadOnlyCollection<AttributeOption> Options => _options.AsReadOnly();

    private CategoryAttribute(Guid id, string attributeName, AttributeInputType inputType,
        Guid categoryId, bool isRequired, bool isFilterable,
        Slug? slug, int sortOrder, Guid? defaultUnitId, Guid? allowedGroupUnitId)
        : base(id)
    {
        AttributeName = attributeName;
        InputType = inputType;
        CategoryId = categoryId;
        IsRequired = isRequired;
        IsFilterable = isFilterable;
        Slug = slug;
        SortOrder = sortOrder;
        DefaultUnitId = defaultUnitId;
        AllowedGroupUnitId = allowedGroupUnitId;
    }

    private CategoryAttribute() { }

    public static CategoryAttribute Create(Guid id, string attributeName, AttributeInputType inputType,
        Guid categoryId, bool isRequired = false, bool isFilterable = false,
        string? slug = null, int sortOrder = 0, Guid? defaultUnitId = null, Guid? allowedGroupUnitId = null)
    {
        return new CategoryAttribute(id, attributeName, inputType, categoryId,
            isRequired, isFilterable, Slug.CreateNullable(slug), sortOrder, defaultUnitId, allowedGroupUnitId);
    }

    internal void Update(string attributeName, bool isRequired, bool isFilterable, int sortOrder)
    {
        AttributeName = attributeName;
        IsRequired = isRequired;
        IsFilterable = isFilterable;
        SortOrder = sortOrder;
        UpdateLastModified();
    }

    internal AttributeOption AddOption(Guid optionId, string label, string value, int sortOrder = 0)
    {
        if (_options.Any(o => o.Value.Equals(value, StringComparison.OrdinalIgnoreCase)))
            throw new DomainException(CatalogErrors.AttributeOptionDuplicateValue.Code, CatalogErrors.AttributeOptionDuplicateValue.Message);

        var option = AttributeOption.Create(optionId, Id, label, value, sortOrder);
        _options.Add(option);
        UpdateLastModified();
        return option;
    }

    internal void RemoveOption(AttributeOption option)
    {
        option.Delete();
        _options.Remove(option);
        UpdateLastModified();
    }

    internal void AssignToGroup(Guid? attributeGroupId)
    {
        AttributeGroupId = attributeGroupId;
        UpdateLastModified();
    }

    public override void Delete()
    {
        foreach (var option in _options)
            option.Delete();
        base.Delete();
    }
}
