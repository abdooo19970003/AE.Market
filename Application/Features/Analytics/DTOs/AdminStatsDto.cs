namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record AdminStatsDto(
    int TotalProducts,
    int ActiveStock,
    decimal AveragePrice,
    int TotalCategories,
    IReadOnlyList<CategoryProductCountDto> ProductsByCategory);

public sealed record CategoryProductCountDto(string Name, int Count);
