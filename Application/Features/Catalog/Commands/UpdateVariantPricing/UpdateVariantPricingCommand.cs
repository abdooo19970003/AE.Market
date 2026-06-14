using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Catalog.DTOs;

namespace AE.Market.Application.Features.Catalog.Commands.UpdateVariantPricing;

public sealed record UpdateVariantPricingCommand(
    Guid ProductId,
    Guid VariantId,
    decimal SalePrice
) : ICommand<VariantDto>;
