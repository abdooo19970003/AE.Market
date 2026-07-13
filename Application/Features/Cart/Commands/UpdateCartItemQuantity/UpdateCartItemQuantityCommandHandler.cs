using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.DTOs;
using AE.Market.Application.Features.Cart.Specs;
using AE.Market.Domain.Aggregates.Cart.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Commands.UpdateCartItemQuantity;

internal sealed class UpdateCartItemQuantityCommandHandler(
    IRepository<CartAggregate> repo
) : IRequestHandler<UpdateCartItemQuantityCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        var cart = await FindCartAsync(request.UserId, request.SessionId, cancellationToken);
        if (cart is null)
            return Result<CartDto>.Fail(CartErrors.CartNotFound);

        try
        {
            cart.UpdateItemQuantity(request.VariantId, request.Quantity);
        }
        catch (DomainException ex)
        {
            var error = ex.Message.Contains("not found")
                ? CartErrors.CartItemNotFound
                : CartErrors.InvalidQuantity;

            return Result<CartDto>.Fail(new Error(error.Code, ex.Message));
        }

        return Result<CartDto>.Success(MapToDto(cart));
    }

    private async Task<CartAggregate?> FindCartAsync(Guid? userId, Guid? sessionId, CancellationToken ct)
    {
        if (userId.HasValue)
            return await repo.FirstOrDefaultAsync(new CartByUserIdSpec(userId.Value), ct);

        if (sessionId.HasValue)
            return await repo.FirstOrDefaultAsync(new CartBySessionIdSpec(sessionId.Value), ct);

        return null;
    }

    private static CartDto MapToDto(CartAggregate cart)
    {
        return new CartDto
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
        };
    }
}
