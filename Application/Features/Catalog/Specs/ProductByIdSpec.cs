using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class ProductByIdSpec(Guid id) : BaseSpecification<Product>(p => p.Id == id)
{
    public ProductByIdSpec(Guid id, bool includeChildren) : this(id)
    {
        if (includeChildren)
        {
            AddInclude(p => p.Variants);
            AddInclude(p => p.Images);
            AddInclude(p => p.Tags);
            AddInclude(p => p.AttributeValues);
        }
    }
}
