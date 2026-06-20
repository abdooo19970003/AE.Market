using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class RelatedProductsByTypeSpec : BaseSpecification<Product>
{
    public RelatedProductsByTypeSpec(Guid productId, RelationType type, bool? isActive = null)
        : base(p => p.Relations.Any(r => r.ProductId == productId && r.Type == type)
                    && (isActive == null || p.IsActive == isActive))
    {
    }

    public RelatedProductsByTypeSpec(Guid productId, RelationType type, int page, int pageSize, bool? isActive = null)
        : base(p => p.Relations.Any(r => r.ProductId == productId && r.Type == type)
                    && (isActive == null || p.IsActive == isActive))
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
