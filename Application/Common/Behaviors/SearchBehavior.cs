using System.Diagnostics;
using System.Text.Json;
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Common.Behaviors;

internal sealed class SearchBehavior<TRequest, TResponse>(
    IElasticsearchService esService,
    ICurrentUser currentUser
) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is SearchProductsQuery productsQuery)
            return await HandleSearchProducts(productsQuery, cancellationToken);

        if (request is SearchSuggestQuery suggestQuery)
            return await HandleSearchSuggest(suggestQuery, cancellationToken);

        return await next(cancellationToken);
    }

    private async Task<TResponse> HandleSearchProducts(SearchProductsQuery query, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        var result = await esService.SearchProductsAsync(query, ct);
        sw.Stop();

        _ = LogAnalyticsAsync(query.Q ?? "", query, result.TotalCount, sw.ElapsedMilliseconds, ct);

        return (TResponse)(object)Result<SearchProductsResult>.Success(result);
    }

    private async Task<TResponse> HandleSearchSuggest(SearchSuggestQuery query, CancellationToken ct)
    {
        var result = await esService.SearchSuggestAsync(query, ct);
        return (TResponse)(object)Result<SearchSuggestResult>.Success(result);
    }

    private async Task LogAnalyticsAsync(string searchText, SearchProductsQuery query, int resultCount, long latencyMs, CancellationToken ct)
    {
        try
        {
            var filters = JsonSerializer.Serialize(new
            {
                query.CategoryId,
                query.BrandId,
                query.MinPrice,
                query.MaxPrice,
                query.Status,
                query.InStock,
                query.TagIds,
                query.AttributeFilters
            });

            var entry = new SearchLogEntry
            {
                Query = searchText,
                Filters = filters,
                ResultCount = resultCount,
                LatencyMs = (int)latencyMs,
                UserId = currentUser.IsAuthenticated ? currentUser.UserId : null,
                Timestamp = DateTime.UtcNow
            };

            await esService.LogSearchQueryAsync(entry, ct);
        }
        catch
        {
            // Fire-and-forget — analytics failure must not break search
        }
    }
}
