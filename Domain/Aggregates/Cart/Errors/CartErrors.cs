using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Cart.Errors;

public static class CartErrors
{
    public static readonly Error CartNotFound = new(
        "Cart.NotFound",
        "The cart was not found."
    );

    public static readonly Error CartItemNotFound = new(
        "Cart.Item.NotFound",
        "The cart item was not found."
    );

    public static readonly Error InvalidQuantity = new(
        "Cart.Item.InvalidQuantity",
        "Quantity must be between 1 and 999."
    );

    public static readonly Error CartAlreadyMerged = new(
        "Cart.AlreadyMerged",
        "The cart has already been merged."
    );
}
