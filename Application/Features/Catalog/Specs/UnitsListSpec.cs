using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Catalog.Specs;

public sealed class UnitsListSpec : BaseSpecification<DomainUnit>
{
    public UnitsListSpec()
    {
    }

    public UnitsListSpec(int page, int pageSize)
    {
        SetPagination((page - 1) * pageSize, pageSize);
    }
}
