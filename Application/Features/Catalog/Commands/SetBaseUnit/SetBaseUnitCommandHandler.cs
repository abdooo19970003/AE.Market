using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Common.Abstracts;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.SetBaseUnit;

internal sealed class SetBaseUnitCommandHandler(
    IRepository<DomainUnit> repo,
    IMapper mapper
) : IRequestHandler<SetBaseUnitCommand, Result<UnitDto>>
{
    private static readonly Error UnitNotFound = new("Catalog.Unit.NotFound", "The specified unit was not found.");

    public async Task<Result<UnitDto>> Handle(SetBaseUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (unit is null)
            return Result<UnitDto>.Fail(UnitNotFound);

        unit.SetAsBaseUnit();

        var dto = mapper.Map<UnitDto>(unit);
        return Result<UnitDto>.Success(dto);
    }
}
