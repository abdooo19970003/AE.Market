namespace AE.Market.Application.Features.Catalog.DTOs;

public sealed record PaginationParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public bool IsActive { get; init; } = true;

    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
