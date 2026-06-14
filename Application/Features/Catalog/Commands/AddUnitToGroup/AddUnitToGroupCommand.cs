using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.AddUnitToGroup;

public sealed record AddUnitToGroupCommand(
    Guid GroupUnitId,
    string UnitName,
    string Abbreviation,
    bool IsBaseUnit,
    decimal ExchangeRateToBaseUnit
) : ICommand<UnitDto>;
