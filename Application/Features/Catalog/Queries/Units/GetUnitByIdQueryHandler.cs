using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Common.Abstracts;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Queries.Units;

internal sealed class GetUnitByIdQueryHandler(
    IReadRepository<DomainUnit> repo,
    IMapper mapper
) : IRequestHandler<GetUnitByIdQuery, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(GetUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (unit is null)
            return Result<UnitDto>.Fail(new Error("Catalog.Unit.NotFound", "The specified unit was not found."));

        var dto = mapper.Map<UnitDto>(unit);
        return Result<UnitDto>.Success(dto);
    }
}
