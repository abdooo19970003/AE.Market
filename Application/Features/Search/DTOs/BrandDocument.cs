namespace AE.Market.Application.Features.Search.DTOs;

public sealed record BrandDocument
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? ShortDescription { get; init; }
    public string? LongDescription { get; init; }
    public string? LogoUrl { get; init; }
    public bool IsActive { get; init; }
    public int ProductCount { get; init; }
}
