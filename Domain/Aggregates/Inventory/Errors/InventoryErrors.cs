using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory.Errors;

public static class InventoryErrors
{
    public static readonly Error InventoryItemNotFound = new(
        "Inventory.Item.NotFound",
        "The specified inventory item was not found."
    );

    public static readonly Error InsufficientStock = new(
        "Inventory.Item.InsufficientStock",
        "Insufficient stock for the requested operation."
    );

    public static readonly Error CannotReleaseMoreThanReserved = new(
        "Inventory.Item.CannotReleaseMoreThanReserved",
        "Cannot release more than the reserved quantity."
    );

    public static readonly Error ReservationExceedsAvailable = new(
        "Inventory.Item.ReservationExceedsAvailable",
        "Reservation quantity exceeds available stock."
    );

    public static readonly Error InventoryItemAlreadyExists = new(
        "Inventory.Item.AlreadyExists",
        "An inventory item already exists for this variant."
    );
}
