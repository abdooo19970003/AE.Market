using AE.Market.Domain.Common.Abstracts;

namespace AE.Market.Domain.Aggregates.Inventory;

public sealed record ShippingDimensions : IValueObject
{
    private ShippingDimensions(
        int weightInGrams,
        int longInCentimeter,
        int heightInCentimeter,
        int widthInCentimeter
    )
    {
        WeightInGrams = weightInGrams;
        LongInCentimeter = longInCentimeter;
        HeightInCentimeter = heightInCentimeter;
        WidthInCentimeter = widthInCentimeter;
    }

    public static ShippingDimensions Create(
        int weightInGrams,
        int longInCentimeter,
        int heightInCentimeter,
        int widthInCentimeter
    ) => new(weightInGrams, longInCentimeter, heightInCentimeter, widthInCentimeter);

    public int WeightInGrams { get; init; }
    public int LongInCentimeter { get; init; }
    public int HeightInCentimeter { get; init; }
    public int WidthInCentimeter { get; init; }
}
