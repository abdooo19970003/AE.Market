using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Catalog.Products;

public sealed class ProductRelation : BaseEntity
{
    public Guid ProductId { get; private set; }
    public Guid RelatedProductId { get; private set; }
    public RelationType Type { get; private set; }
    public int SortOrder { get; private set; }

    private ProductRelation(
        Guid id,
        Guid productId,
        Guid relatedProductId,
        RelationType type,
        int sortOrder)
        : base(id)
    {
        ProductId = productId;
        RelatedProductId = relatedProductId;
        Type = type;
        SortOrder = sortOrder;
    }

    private ProductRelation() { }

    internal static ProductRelation Create(
        Guid id,
        Guid productId,
        Guid relatedProductId,
        RelationType type,
        int sortOrder = 0)
    {
        return new ProductRelation(id, productId, relatedProductId, type, sortOrder);
    }

    public void UpdateSortOrder(int sortOrder)
    {
        SortOrder = sortOrder;
        UpdateLastModified();
    }

    public override void Delete()
    {
        base.Delete();
    }
}
