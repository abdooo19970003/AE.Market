using AE.Market.Application.Common.Abstracts;
using AE.Market.Domain.Aggregates.Prices;

namespace AE.Market.Application.Features.Pricing.Commands.DeletePrice;

public sealed record DeletePriceCommand(Guid VariantId, Guid? MarketplaceId, PriceType Type) : ICommand;
