using AE.Market.Application.Features.Search.DTOs;

namespace AE.Market.Application.Common.Interfaces;

public interface IElasticsearchService
{
    Task<EnsureIndicesResult> EnsureIndicesAsync(CancellationToken ct = default);
    Task<SearchProductsResult> SearchProductsAsync(SearchProductsQuery query, CancellationToken ct = default);
    Task<SearchSuggestResult> SearchSuggestAsync(SearchSuggestQuery query, CancellationToken ct = default);
    Task IndexProductAsync(ProductDocument document, CancellationToken ct = default);
    Task UpdateProductAsync(Guid productId, ProductDocument document, CancellationToken ct = default);
    Task DeleteProductAsync(Guid productId, CancellationToken ct = default);
    Task IndexBrandAsync(BrandDocument document, CancellationToken ct = default);
    Task UpdateBrandAsync(Guid brandId, BrandDocument document, CancellationToken ct = default);
    Task DeleteBrandAsync(Guid brandId, CancellationToken ct = default);
    Task LogSearchQueryAsync(SearchLogEntry entry, CancellationToken ct = default);
    Task<int> BulkIndexProductsAsync(IEnumerable<ProductDocument> documents, CancellationToken ct = default);
    Task<int> BulkIndexBrandsAsync(IEnumerable<BrandDocument> documents, CancellationToken ct = default);
}

public sealed record EnsureIndicesResult(bool ProductsCreated, bool BrandsCreated, bool SearchLogsCreated);
