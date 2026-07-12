using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Units;

internal sealed class GetUnitsListQueryHandler(
    IReadRepository<DomainUnit> repo,
    IMapper mapper
) : IRequestHandler<GetUnitsListQuery, Result<PaginatedList<UnitDto>>>
{
    public async Task<Result<PaginatedList<UnitDto>>> Handle(GetUnitsListQuery request, CancellationToken cancellationToken)
    {
        var spec = new BaseSpecification<DomainUnit>();
        var totalCount = await repo.CountAsync(spec, cancellationToken);
        spec.SetPagination((request.Page - 1) * request.PageSize, request.PageSize);

        var items = await repo.ListWithSpecAsync(spec, cancellationToken);
        var dtos = mapper.Map<List<UnitDto>>(items);

        var result = new PaginatedList<UnitDto>
        {
            Items = dtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };

        return Result<PaginatedList<UnitDto>>.Success(result);
    }
}
