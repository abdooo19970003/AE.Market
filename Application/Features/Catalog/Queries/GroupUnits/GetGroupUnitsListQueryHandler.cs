using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.GroupUnits;

internal sealed class GetGroupUnitsListQueryHandler(
    IReadRepository<GroupUnit> repo,
    IMapper mapper
) : IRequestHandler<GetGroupUnitsListQuery, Result<List<GroupUnitDto>>>
{
    public async Task<Result<List<GroupUnitDto>>> Handle(GetGroupUnitsListQuery request, CancellationToken cancellationToken)
    {
        var groupUnits = await repo.ListAsync(cancellationToken);
        var dtos = mapper.Map<List<GroupUnitDto>>(groupUnits);
        return Result<List<GroupUnitDto>>.Success(dtos);
    }
}
