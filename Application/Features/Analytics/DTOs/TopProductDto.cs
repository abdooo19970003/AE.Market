namespace AE.Market.Application.Features.Analytics.DTOs;

public sealed record TopProductDto(Guid ProductId, string Name, int ViewCount);
