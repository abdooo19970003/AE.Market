namespace AE.Market.Application.Features.Pricing.DTOs;

public sealed record PriceDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public bool IsActive { get; set; }
}
