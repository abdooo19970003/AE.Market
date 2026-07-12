using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Domain.Aggregates.Prices;

namespace AE.Market.Application.Features.Pricing.Commands.SetInitialPrice;

public sealed record SetInitialPriceCommand(
    Guid VariantId,
    PriceType Type,
    decimal Amount,
    string CurrencyCode
) : ICommand<PriceDto>;
