using AE.Market.Domain.Aggregates.Catalog.Attributes;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class AttributeGroupsListSpec : BaseSpecification<AttributeGroup>
{
    public AttributeGroupsListSpec()
    {
    }

    public AttributeGroupsListSpec(int page, int pageSize)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
