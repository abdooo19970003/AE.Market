using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductsByTagSpec : BaseSpecification<Product>
{
    public ProductsByTagSpec(string tagSlug, bool? isActive = null)
        : base(p => p.Tags.Any(t => t.Slug.Value == tagSlug)
                    && (isActive == null || p.IsActive == isActive))
    {
    }

    public ProductsByTagSpec(string tagSlug, int page, int pageSize, bool? isActive = null)
        : base(p => p.Tags.Any(t => t.Slug.Value == tagSlug)
                    && (isActive == null || p.IsActive == isActive))
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
