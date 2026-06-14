using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class GroupUnitByIdSpec(Guid id) : BaseSpecification<GroupUnit>(g => g.Id == id)
{
    public GroupUnitByIdSpec(Guid id, bool includeUnits) : this(id)
    {
        if (includeUnits)
            AddInclude(g => g.Units);
    }
}
