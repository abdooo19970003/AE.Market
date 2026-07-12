using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateProductTaxCode;

public sealed record UpdateProductTaxCodeCommand(
    Guid Id,
    string Code,
    string Type,
    string? PerformanceLocationRequirement,
    string Name,
    string Description
) : ICommand<ProductTaxCodeDto>;
