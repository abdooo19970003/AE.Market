using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Domain.Aggregates.Prices;

namespace AE.Market.Application.Features.Pricing.Commands.UpdatePrice;

public sealed record UpdatePriceCommand(
    Guid VariantId,
    Guid MarketplaceId,
    PriceType Type,
    decimal Amount,
    string CurrencyCode,
    PriceChangeReason Reason
) : ICommand<PriceDto>;
