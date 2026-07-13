namespace AE.Market.Domain.Aggregates.Pricing;

public interface IPriceCalculator
{
    Task<FinalPrice> CalculateAsync(
        Guid variantId,
        int quantity,
        Guid marketplaceId,
        CancellationToken ct = default);
}
