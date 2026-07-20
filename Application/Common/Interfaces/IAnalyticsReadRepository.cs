using AE.Market.Application.Features.Analytics.DTOs;

namespace AE.Market.Application.Common.Interfaces;

public interface IAnalyticsReadRepository
{
    Task<AdminStatsDto> GetAdminStatsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<TopProductDto>> GetTopProductsAsync(int days, int top, CancellationToken cancellationToken);
    Task<IReadOnlyList<TopSearchDto>> GetTopSearchesAsync(int days, int top, CancellationToken cancellationToken);
}
