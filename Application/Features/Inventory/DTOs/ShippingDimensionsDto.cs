namespace AE.Market.Application.Features.Inventory.DTOs;

public sealed record ShippingDimensionsDto(
    int WeightInGrams,
    int LongInCentimeter,
    int HeightInCentimeter,
    int WidthInCentimeter
);
