using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class TagsListSpec : BaseSpecification<Tag>
{
    public TagsListSpec()
    {
    }

    public TagsListSpec(int page, int pageSize)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
