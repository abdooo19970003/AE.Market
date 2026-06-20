using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class CategoryAttributesListSpec : BaseSpecification<CategoryAttribute>
{
    public CategoryAttributesListSpec()
    {
    }

    public CategoryAttributesListSpec(int page, int pageSize)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
