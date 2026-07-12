using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Aggregates.Prices;

namespace AE.Market.Application.Features.Pricing.Commands.ActivatePrice;

public sealed record ActivatePriceCommand(
    Guid VariantId,
    PriceType Type,
    DateTime? NewValidFrom
) : ICommand;
