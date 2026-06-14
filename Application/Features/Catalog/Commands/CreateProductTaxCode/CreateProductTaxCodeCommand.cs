using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.CreateProductTaxCode;

public sealed record CreateProductTaxCodeCommand(
    string Code,
    string Type,
    string? PerformanceLocationRequirement,
    string Name,
    string Description
) : ICommand<ProductTaxCodeDto>;
