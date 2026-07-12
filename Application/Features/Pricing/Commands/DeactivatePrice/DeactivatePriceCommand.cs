using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Aggregates.Prices;

namespace AE.Market.Application.Features.Pricing.Commands.DeactivatePrice;

public sealed record DeactivatePriceCommand(Guid VariantId, Guid? MarketplaceId, PriceType Type) : ICommand;
