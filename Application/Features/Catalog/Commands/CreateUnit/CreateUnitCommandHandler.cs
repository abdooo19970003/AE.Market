using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Catalog.DTOs;
using AE.Market.Domain.Common.Abstracts;
using DomainUnit = AE.Market.Domain.Aggregates.Catalog.Units.Unit;
using DomainFormulaType = AE.Market.Domain.Aggregates.Catalog.Units.FormulaType;
using MediatR;

namespace AE.Market.Application.Features.Catalog.Commands.CreateUnit;

internal sealed class CreateUnitCommandHandler(
    IRepository<DomainUnit> repo,
    IMapper mapper
) : IRequestHandler<CreateUnitCommand, Result<UnitDto>>
{
    public async Task<Result<UnitDto>> Handle(CreateUnitCommand request, CancellationToken cancellationToken)
    {
        var formulaType = (DomainFormulaType)Enum.Parse(typeof(DomainFormulaType), request.FormulaType);

        var unit = DomainUnit.Create(
            Guid.NewGuid(),
            request.UnitName,
            request.Abbreviation,
            request.GroupUnitId,
            request.IsBaseUnit,
            request.ExchangeRateToBaseUnit,
            formulaType,
            request.ConversionFormulaDescription
        );

        await repo.AddAsync(unit, cancellationToken);

        var dto = mapper.Map<UnitDto>(unit);
        return Result<UnitDto>.Success(dto);
    }
}
