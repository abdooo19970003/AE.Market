using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateUnit;

public sealed record UpdateUnitCommand(
    Guid Id,
    string UnitName,
    string Abbreviation,
    decimal ExchangeRateToBaseUnit,
    string? ConversionFormulaDescription
) : ICommand<UnitDto>;
