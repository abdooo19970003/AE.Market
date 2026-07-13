using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.DTOs;
using AE.Market.Application.Features.Cart.Specs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Queries.GetCart;

internal sealed class GetCartQueryHandler(
    IReadRepository<CartAggregate> repo
) : IRequestHandler<GetCartQuery, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        CartAggregate? cart = null;

        if (request.UserId.HasValue)
            cart = await repo.FirstOrDefaultAsync(
                new CartByUserIdSpec(request.UserId.Value),
                cancellationToken);

        if (cart is null && request.SessionId.HasValue)
            cart = await repo.FirstOrDefaultAsync(
                new CartBySessionIdSpec(request.SessionId.Value),
                cancellationToken);

        if (cart is null)
            return Result<CartDto>.Success(new CartDto
            {
                Items = [],
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            });

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
}
