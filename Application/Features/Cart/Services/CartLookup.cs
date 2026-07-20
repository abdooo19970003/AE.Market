using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.Specs;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Cart.Services;

internal sealed class CartLookup(
    IReadRepository<CartAggregate> cartReadRepo,
    IRepository<CartAggregate> cartRepo
) : ICartLookup
{
    public async Task<(Guid CartId, IReadOnlyList<CartItemInfo> Items)?> GetCartForOrderAsync(
        Guid userId, CancellationToken ct = default)
    {
        var cart = await cartReadRepo.FirstOrDefaultAsync(new CartByUserIdSpec(userId), ct);
        if (cart is null || cart.Items.Count == 0)
            return null;

        var items = cart.Items
            .Select(i => new CartItemInfo(i.VariantId, i.Quantity))
            .ToList();

        return (cart.Id, items);
    }

    public async Task ClearCartAsync(Guid cartId, CancellationToken ct = default)
    {
        var cart = await cartRepo.GetBySpecWithTrackingAsync(new CartByIdSpec(cartId), ct);
        cart?.ClearCart();
    }
}
