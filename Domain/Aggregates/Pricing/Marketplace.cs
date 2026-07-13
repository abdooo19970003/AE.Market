using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.ValueObjects;

namespace AE.Market.Domain.Aggregates.Pricing;

public sealed class Marketplace : BaseEntity, IAggregateRoot
{
    public string Code { get; private set; } = string.Empty;
    public string Region { get; private set; } = string.Empty;
    public Currency PreferredCurrency { get; private set; } = default!;

    private Marketplace() { }

    private Marketplace(Guid id, string code, string region, Currency preferredCurrency)
        : base(id)
    {
        Code = code;
        Region = region;
        PreferredCurrency = preferredCurrency;
    }

    public static Marketplace Create(Guid id, string code, string region, Currency preferredCurrency)
    {
        return new Marketplace(id, code, region, preferredCurrency);
    }

    public void UpdateDetails(string code, string region)
    {
        Code = code;
        Region = region;
        UpdateLastModified();
    }
}
