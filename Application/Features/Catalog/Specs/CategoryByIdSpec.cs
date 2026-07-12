using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class CategoryByIdSpec(Guid id) : BaseSpecification<Category>(c => c.Id == id)
{
    public CategoryByIdSpec(Guid id, bool includeChildren) : this(id)
    {
        if (includeChildren)
        {
            AddInclude(c => c.SubCategories);
            AddInclude(c => c.Attributes);
        }
    }
}
