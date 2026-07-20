using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Features.Analytics.Queries.GetTopProducts;

public sealed record GetTopProductsQuery(
    int Days = 30,
    int Top = 10
) : IBaseQuery<IReadOnlyList<TopProductDto>>;
