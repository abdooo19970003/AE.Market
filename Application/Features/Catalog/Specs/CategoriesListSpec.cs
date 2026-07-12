using AE.Market.Domain.Aggregates.Catalog;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class CategoriesListSpec : BaseSpecification<Category>
{
    public CategoriesListSpec()
    {
    }

    public CategoriesListSpec(int page, int pageSize)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
