using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Commands.SetInitialPrice;

internal sealed class SetInitialPriceCommandHandler(
    IRepository<Price> priceRepo,
    IRepository<PriceHistory> historyRepo,
    IMapper mapper
) : IRequestHandler<SetInitialPriceCommand, Result<PriceDto>>
{
    public async Task<Result<PriceDto>> Handle(SetInitialPriceCommand request, CancellationToken cancellationToken)
    {
        var existingActive = await priceRepo.FirstOrDefaultAsync(
            new ActivePriceByVariantAndTypeSpec(request.VariantId, request.MarketplaceId, request.Type),
            cancellationToken);

        if (existingActive is not null)
            return Result<PriceDto>.Fail(PriceErrors.DuplicateActiveSalePrice);

        var money = Money.FromDecimal(request.Amount, request.CurrencyCode);
        var price = Price.Create(Guid.NewGuid(), request.VariantId, request.MarketplaceId, request.Type, money);

        await priceRepo.AddAsync(price, cancellationToken);

        var history = PriceHistory.Create(
            Guid.NewGuid(),
            price.Id,
            price.VariantId,
            Money.Zero(money.Currency),
            money,
            PriceChangeReason.Initial);

        await historyRepo.AddAsync(history, cancellationToken);

        var dto = mapper.Map<PriceDto>(price);
        return Result<PriceDto>.Success(dto);
    }
}
