using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class RelatedProductsByTypeSpec : BaseSpecification<Product>
{
    public RelatedProductsByTypeSpec(Guid productId, RelationType type, ProductStatus? status = null)
        : base(p => p.Relations.Any(r => r.ProductId == productId && r.Type == type)
                    && (status == null || p.Status == status))
    {
    }

    public RelatedProductsByTypeSpec(Guid productId, RelationType type, int page, int pageSize, ProductStatus? status = null)
        : base(p => p.Relations.Any(r => r.ProductId == productId && r.Type == type)
                    && (status == null || p.Status == status))
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
