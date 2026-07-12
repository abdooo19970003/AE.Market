namespace AE.Market.Application.Features.Pricing.DTOs;

public sealed record MarginDto
{
    public Guid VariantId { get; set; }
    public decimal CostPrice { get; set; }
    public decimal SellPrice { get; set; }
    public string Currency { get; set; } = string.Empty;
    public decimal MarginPercentage { get; set; }
    public decimal Profit { get; set; }
}
