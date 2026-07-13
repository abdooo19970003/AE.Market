using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.DTOs;
using AE.Market.Application.Features.Cart.Specs;
using AE.Market.Domain.Aggregates.Cart.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Commands.MergeGuestCart;

internal sealed class MergeGuestCartCommandHandler(
    IRepository<CartAggregate> repo
) : IRequestHandler<MergeGuestCartCommand, Result<CartDto>>
{
    public async Task<Result<CartDto>> Handle(MergeGuestCartCommand request, CancellationToken cancellationToken)
    {
        var guestCart = await repo.FirstOrDefaultAsync(
            new CartBySessionIdSpec(request.SessionId),
            cancellationToken);

        var userCart = await repo.FirstOrDefaultAsync(
            new CartByUserIdSpec(request.UserId),
            cancellationToken);

        if (guestCart is null)
            return Result<CartDto>.Fail(CartErrors.CartNotFound);

        userCart ??= CartAggregate.CreateForUser(Guid.NewGuid(), request.UserId);

        try
        {
            userCart.MergeFrom(guestCart);
        }
        catch (DomainException ex)
        {
            return Result<CartDto>.Fail(new Error("Cart.AlreadyMerged", ex.Message));
        }

        if (userCart == null || userCart.Id == Guid.Empty)
            await repo.AddAsync(userCart!, cancellationToken);

        return Result<CartDto>.Success(MapToDto(userCart));
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
