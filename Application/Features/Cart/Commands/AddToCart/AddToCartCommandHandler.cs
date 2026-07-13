using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.DTOs;
using AE.Market.Application.Features.Cart.Specs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Commands.AddToCart;

internal sealed class AddToCartCommandHandler(
    IRepository<CartAggregate> repo
) : IRequestHandler<AddToCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(AddToCartCommand request, CancellationToken cancellationToken)
    {
        var cart = await ResolveCartAsync(request, cancellationToken);

        cart.AddItem(Guid.NewGuid(), request.VariantId, request.Quantity);

        return Result<CartDto>.Success(new CartDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            SessionId = cart.SessionId,
            Status = cart.Status.ToString(),
            Items = cart.Items.Select(i => new CartItemDto
            {
                Id = i.Id,
                VariantId = i.VariantId,
                Quantity = i.Quantity,
                AddedAt = i.AddedAt
            }).ToList(),
            CreatedAt = cart.CreatedAt
        });
    }

    private async Task<CartAggregate> ResolveCartAsync(AddToCartCommand request, CancellationToken cancellationToken)
    {
        if (request.UserId.HasValue)
        {
            var existing = await repo.FirstOrDefaultAsync(
                new CartByUserIdSpec(request.UserId.Value),
                cancellationToken);

            if (existing is not null)
                return existing;

            var cart = CartAggregate.CreateForUser(Guid.NewGuid(), request.UserId.Value);
            await repo.AddAsync(cart, cancellationToken);
            return cart;
        }

        if (request.SessionId.HasValue)
        {
            var existing = await repo.FirstOrDefaultAsync(
                new CartBySessionIdSpec(request.SessionId.Value),
                cancellationToken);

            if (existing is not null)
                return existing;

            var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), request.SessionId.Value);
            await repo.AddAsync(cart, cancellationToken);
            return cart;
        }

        throw new InvalidOperationException("Either UserId or SessionId must be provided.");
    }
}
