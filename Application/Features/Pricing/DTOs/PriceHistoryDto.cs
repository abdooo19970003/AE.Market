namespace AE.Market.Application.Features.Pricing.DTOs;

public sealed record PriceHistoryDto
{
    public Guid Id { get; set; }
    public Guid PriceId { get; set; }
    public Guid VariantId { get; set; }
    public decimal OldAmount { get; set; }
    public decimal NewAmount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public Guid? ChangedBy { get; set; }
    public DateTime ChangedAt { get; set; }
}
