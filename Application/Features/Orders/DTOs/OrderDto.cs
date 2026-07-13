namespace AE.Market.Application.Features.Orders.DTOs;

public sealed record OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTime PlacedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public sealed record OrderItemDto
{
    public Guid Id { get; set; }
    public Guid VariantId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string VariantName { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public decimal SellPrice { get; set; }
    public int Quantity { get; set; }
}
