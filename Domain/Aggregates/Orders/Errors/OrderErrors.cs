using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Orders.Errors;

public static class OrderErrors
{
    public static readonly Error OrderNotFound = new(
        "Orders.NotFound",
        "The order was not found.");

    public static readonly Error EmptyOrder = new(
        "Orders.Empty",
        "Cannot place an order with no items.");

    public static readonly Error InvalidQuantity = new(
        "Orders.Item.InvalidQuantity",
        "Quantity must be between 1 and 999.");

    public static readonly Error AlreadyCancelled = new(
        "Orders.AlreadyCancelled",
        "The order has already been cancelled.");

    public static readonly Error NotCancellable = new(
        "Orders.NotCancellable",
        "Only pending orders can be cancelled.");
}
