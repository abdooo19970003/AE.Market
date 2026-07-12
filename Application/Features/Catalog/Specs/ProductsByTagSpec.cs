using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.ValueObjects;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductsByTagSpec : BaseSpecification<Product>
{
    public ProductsByTagSpec(string tagSlug, ProductStatus? status = null)
        : base(p => p.Tags.Any(t => t.Slug.Value == tagSlug)
                    && (status == null || p.Status == status))
    {
    }

    public ProductsByTagSpec(string tagSlug, int page, int pageSize, ProductStatus? status = null)
        : base(p => p.Tags.Any(t => t.Slug.Value == tagSlug)
                    && (status == null || p.Status == status))
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
