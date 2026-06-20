using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateUnit;

public sealed record CreateUnitCommand(
    string UnitName,
    string Abbreviation,
    Guid GroupUnitId,
    bool IsBaseUnit,
    decimal ExchangeRateToBaseUnit,
    string FormulaType,
    string? ConversionFormulaDescription
) : ICommand<UnitDto>;
