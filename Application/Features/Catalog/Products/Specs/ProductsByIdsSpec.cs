using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Products.Specs;

public sealed class ProductsByIdsSpec : BaseSpecification<Product>
{
    public ProductsByIdsSpec(IReadOnlyList<Guid> ids)
        : base(p => ids.Contains(p.Id))
    {
    }
}
