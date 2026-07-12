using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Common.Abstracts;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateUnit;

internal sealed class UpdateUnitCommandHandler(
    IRepository<DomainUnit> repo,
    IMapper mapper
) : IRequestHandler<UpdateUnitCommand, Result<UnitDto>>
{
    private static readonly Error UnitNotFound = new("Catalog.Unit.NotFound", "The specified unit was not found.");

    public async Task<Result<UnitDto>> Handle(UpdateUnitCommand request, CancellationToken cancellationToken)
    {
        var unit = await repo.GetByIdWithTrackingAsync(request.Id, cancellationToken);
        if (unit is null)
            return Result<UnitDto>.Fail(UnitNotFound);

        if (!unit.IsBaseUnit)
            unit.UpdateExchangeRate(request.ExchangeRateToBaseUnit);

        var dto = mapper.Map<UnitDto>(unit);
        return Result<UnitDto>.Success(dto);
    }
}
