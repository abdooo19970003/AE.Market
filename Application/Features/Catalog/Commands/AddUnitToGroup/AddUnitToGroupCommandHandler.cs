using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Aggregates.Catalog.Errors;
using AE.Market.Domain.Aggregates.Catalog.Units;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.AddUnitToGroup;

internal sealed class AddUnitToGroupCommandHandler(
    IRepository<GroupUnit> repo,
    IMapper mapper
) : IRequestHandler<AddUnitToGroupCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(AddUnitToGroupCommand request, CancellationToken cancellationToken)
    {
        var groupUnit = await repo.GetByIdWithTrackingAsync(request.GroupUnitId, cancellationToken);
        if (groupUnit is null)
            return Result<UnitDto>.Fail(CatalogErrors.GroupUnitNotFound);

        var unit = groupUnit.AddUnit(
            Guid.NewGuid(),
            request.UnitName,
            request.Abbreviation,
            request.IsBaseUnit,
            request.ExchangeRateToBaseUnit
        );

        var dto = mapper.Map<UnitDto>(unit);
        return Result<UnitDto>.Success(dto);
    }
}
