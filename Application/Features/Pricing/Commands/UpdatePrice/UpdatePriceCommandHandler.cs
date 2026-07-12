using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Pricing.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Prices.Errors;
using AE.Market.Domain.Aggregates.Prices.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;
using MediatR;

namespace AE.Market.Application.Features.Pricing.Commands.UpdatePrice;

internal sealed class UpdatePriceCommandHandler(
    IRepository<Price> priceRepo,
    IRepository<PriceHistory> historyRepo,
    IMapper mapper
) : IRequestHandler<UpdatePriceCommand, Result<PriceDto>>
{
    public async Task<Result<PriceDto>> Handle(UpdatePriceCommand request, CancellationToken cancellationToken)
    {
        var activePrice = await priceRepo.FirstOrDefaultAsync(
            new ActivePriceByVariantAndTypeSpec(request.VariantId, request.Type),
            cancellationToken);

        if (activePrice is null)
            return Result<PriceDto>.Fail(PriceErrors.NoActivePrice);

        var oldAmount = activePrice.Deactivate();

        var newMoney = Money.FromDecimal(request.Amount, request.CurrencyCode);
        var newPrice = Price.Create(
            Guid.NewGuid(),
            request.VariantId,
            request.MarketplaceId,
            request.Type,
            newMoney);

        await priceRepo.AddAsync(newPrice, cancellationToken);

        newPrice.AddDomainEvent(new ProductPriceChangedDomainEvent(
            newPrice.Id,
            request.VariantId,
            oldAmount,
            newMoney,
            request.Type));

        var history = PriceHistory.Create(
            Guid.NewGuid(),
            activePrice.Id,
            request.VariantId,
            oldAmount,
            newMoney,
            request.Reason);

        await historyRepo.AddAsync(history, cancellationToken);

        var dto = mapper.Map<PriceDto>(newPrice);
        return Result<PriceDto>.Success(dto);
    }
}
