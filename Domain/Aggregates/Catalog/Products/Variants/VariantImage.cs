using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products.Variants;

public sealed class VariantImage : BaseEntity
{
    public string Url { get; private set; }
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    public Guid VariantId { get; private set; }

    private VariantImage(Guid id, Guid variantId, string url, string? altText, bool isPrimary, int sortOrder)
        : base(id)
    {
        VariantId = variantId;
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    private VariantImage() { }

    internal static VariantImage Create(Guid id, Guid variantId, string url, string? altText, bool isPrimary = false, int sortOrder = 0)
    {
        return new VariantImage(id, variantId, url, altText, isPrimary, sortOrder);
    }

    internal void Update(string url, string? altText, int sortOrder)
    {
        Url = url;
        AltText = altText;
        SortOrder = sortOrder;
        UpdateLastModified();
    }

    internal void SetPrimary()
    {
        IsPrimary = true;
        UpdateLastModified();
    }

    internal void ClearPrimary()
    {
        IsPrimary = false;
        UpdateLastModified();
    }
}
