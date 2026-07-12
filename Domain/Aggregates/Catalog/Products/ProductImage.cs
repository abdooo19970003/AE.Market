using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class ProductImage : BaseEntity
{
    public string Url { get; private set; } = string.Empty;
    public string? AltText { get; private set; }
    public bool IsPrimary { get; private set; }
    public int SortOrder { get; private set; }

    public Guid ProductId { get; private set; }

    private ProductImage(Guid id, Guid productId, string url, string? altText, bool isPrimary, int sortOrder)
        : base(id)
    {
        ProductId = productId;
        Url = url;
        AltText = altText;
        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    private ProductImage() { }

    internal static ProductImage Create(Guid id, Guid productId, string url, string? altText, bool isPrimary = false, int sortOrder = 0)
    {
        return new ProductImage(id, productId, url, altText, isPrimary, sortOrder);
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
