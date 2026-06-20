using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.SetBaseUnit;

public sealed record SetBaseUnitCommand(
    Guid Id
) : ICommand<UnitDto>;
