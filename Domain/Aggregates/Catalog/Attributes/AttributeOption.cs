using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Attributes;

// Options For MultiSelect Attribute (example: pantsSize: XXS, XS, S, M, L, XL, XXL, XXXL)
// Options Values Should be unique under same attribute (having two "S" for pantsSize is wrong)
public sealed class AttributeOption : BaseEntity
{
    public Guid AttributeId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string Value { get; private set; } = string.Empty;
    public int SortOrder { get; private set; }

    private AttributeOption(Guid id, Guid attributeId, string label, string value, int sortOrder)
        : base(id)
    {
        AttributeId = attributeId;
        Label = label;
        Value = value;
        SortOrder = sortOrder;
    }

    private AttributeOption() { }

    internal static AttributeOption Create(Guid id, Guid attributeId, string label, string value, int sortOrder = 0)
    {
        return new AttributeOption(id, attributeId, label, value, sortOrder);
    }

    internal void Update(string label, string value, int sortOrder)
    {
        Label = label;
        Value = value;
        SortOrder = sortOrder;
        UpdateLastModified();
    }
}
