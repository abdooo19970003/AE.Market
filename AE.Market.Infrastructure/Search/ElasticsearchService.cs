using System.Diagnostics;
using System.Text.Json;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Search.DTOs;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Microsoft.Extensions.Logging;

namespace AE.Market.Infrastructure.Search;

internal sealed class ElasticsearchService(
    ElasticsearchClient client,
    ILogger<ElasticsearchService> logger
) : IElasticsearchService
{
    public async Task<EnsureIndicesResult> EnsureIndicesAsync(CancellationToken ct = default)
    {
        var productsExists = await client.Indices.ExistsAsync(ProductIndexDefinition.IndexName, ct);
        bool productsCreated = false;
        if (!productsExists.Exists)
        {
            await client.Indices.CreateAsync(ProductIndexDefinition.GetCreateIndexRequest(), ct);
            productsCreated = true;
            logger.LogInformation("Created ES index: {Index}", ProductIndexDefinition.IndexName);
        }

        var brandsExists = await client.Indices.ExistsAsync(BrandIndexDefinition.IndexName, ct);
        bool brandsCreated = false;
        if (!brandsExists.Exists)
        {
            await client.Indices.CreateAsync(BrandIndexDefinition.GetCreateIndexRequest(), ct);
            brandsCreated = true;
            logger.LogInformation("Created ES index: {Index}", BrandIndexDefinition.IndexName);
        }

        var logsExists = await client.Indices.ExistsAsync(SearchLogIndexDefinition.IndexName, ct);
        bool logsCreated = false;
        if (!logsExists.Exists)
        {
            await client.Indices.CreateAsync(SearchLogIndexDefinition.GetCreateIndexRequest(), ct);
            logsCreated = true;
            logger.LogInformation("Created ES index: {Index}", SearchLogIndexDefinition.IndexName);
        }

        return new EnsureIndicesResult(productsCreated, brandsCreated, logsCreated);
    }

    public async Task<SearchProductsResult> SearchProductsAsync(SearchProductsQuery query, CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();

        var response = await client.SearchAsync<ProductDocument>(s => s
            .Index(ProductIndexDefinition.IndexName)
            .TrackTotalHits(new TrackHits(true))
            .Query(BuildSearchQuery(query))
            .Aggregations(a => a
                .Add("categories", t => t.Terms(te => te.Field(new Field("categoryName")).Size(20)))
                .Add("brands", t => t.Terms(te => te.Field(new Field("brandName")).Size(20)))
                .Add("tags", t => t.Terms(te => te.Field(new Field("tags")).Size(50)))
                .Add("price_ranges", t => t.Histogram(h => h
                    .Field(new Field("listPrice"))
                    .Interval(100)
                    .ExtendedBounds(new ExtendedBoundsFloat { Min = 0, Max = 10000 })
                ))
            )
            .Sort(BuildSort(query.Sort))
            .From((query.Page - 1) * query.Size)
            .Size(query.Size), ct);

        sw.Stop();
        logger.LogDebug("ES search completed in {Ms}ms, found {Total} hits", sw.ElapsedMilliseconds, response.Total);

        if (!response.IsValidResponse)
        {
            var error = response.ElasticsearchServerError?.Error?.Reason ?? "Unknown ES error";
            logger.LogError("ES search failed: {Error}", error);
            throw new InvalidOperationException($"Elasticsearch search failed: {error}");
        }

        var hits = response.Hits;
        var total = (int)response.Total;

        var items = hits.Select(h => new SearchProductsResultItem
        {
            Id = Guid.Parse(h.Id),
            Name = h.Source!.Name,
            Slug = h.Source.Slug,
            ShortDescription = h.Source.ShortDescription,
            ListPrice = h.Source.ListPrice,
            BrandName = h.Source.BrandName,
            CategoryName = h.Source.CategoryName,
            Images = h.Source.Images.Select(i => new SearchResultImage { Url = i.Url, AltText = i.AltText }).ToList(),
            StockStatus = h.Source.StockQuantity > 0 ? "InStock" : (h.Source.AllowBackOrder ? "BackOrder" : "OutOfStock")
        }).ToList();

        var facets = new SearchFacets();
        if (response.Aggregations != null)
        {
            ExtractTermsFacet(response.Aggregations, "categories", facets.Categories);
            ExtractTermsFacet(response.Aggregations, "brands", facets.Brands);
            ExtractTermsFacet(response.Aggregations, "tags", facets.Tags);
            ExtractPriceRanges(response.Aggregations, facets.PriceRanges);
        }

        return new SearchProductsResult
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            Size = query.Size,
            Facets = facets
        };
    }

    public async Task<SearchBrandsResult> SearchBrandsAsync(SearchBrandsQuery query, CancellationToken ct = default)
    {
        var response = await client.SearchAsync<BrandDocument>(s => s
            .Index(BrandIndexDefinition.IndexName)
            .TrackTotalHits(new TrackHits(true))
            .Query(BuildBrandSearchQuery(query))
            .From((query.Page - 1) * query.Size)
            .Size(query.Size), ct);

        if (!response.IsValidResponse)
        {
            var error = response.ElasticsearchServerError?.Error?.Reason ?? "Unknown ES error";
            logger.LogError("ES brand search failed: {Error}", error);
            throw new InvalidOperationException($"Elasticsearch brand search failed: {error}");
        }

        var hits = response.Hits;
        var total = (int)response.Total;

        var items = hits.Select(h => new SearchBrandsResultItem
        {
            Id = Guid.Parse(h.Id),
            Name = h.Source!.Name,
            Slug = h.Source.Slug,
            ShortDescription = h.Source.ShortDescription,
            LogoUrl = h.Source.LogoUrl,
            IsActive = h.Source.IsActive,
            ProductCount = h.Source.ProductCount
        }).ToList();

        return new SearchBrandsResult
        {
            Items = items,
            TotalCount = total,
            Page = query.Page,
            Size = query.Size
        };
    }

    public async Task<SearchSuggestResult> SearchSuggestAsync(SearchSuggestQuery query, CancellationToken ct = default)
    {
        var productTask = client.SearchAsync<ProductDocument>(s => s
            .Index(ProductIndexDefinition.IndexName)
            .Suggest(su => su
                .Text(query.Q)
                .Suggesters(sg => sg
                    .Add("product-suggest", sf => sf
                        .Prefix(query.Q)
                        .Completion(c => c
                            .Field(new Field("name.suggest"))
                            .Fuzzy(f => f.Fuzziness(new Fuzziness("AUTO")))
                            .Size(query.Size)
                        )
                    )
                )
            )
            .Size(0), ct);

        var brandTask = client.SearchAsync<BrandDocument>(s => s
            .Index(BrandIndexDefinition.IndexName)
            .Suggest(su => su
                .Text(query.Q)
                .Suggesters(sg => sg
                    .Add("brand-suggest", sf => sf
                        .Prefix(query.Q)
                        .Completion(c => c
                            .Field(new Field("name.suggest"))
                            .Fuzzy(f => f.Fuzziness(new Fuzziness("AUTO")))
                            .Size(query.Size)
                        )
                    )
                )
            )
            .Size(0), ct);

        await Task.WhenAll(productTask, brandTask);

        var productResponse = productTask.Result;
        var brandResponse = brandTask.Result;

        if (!productResponse.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch product suggest failed: {productResponse.ElasticsearchServerError?.Error?.Reason}");
        if (!brandResponse.IsValidResponse)
            throw new InvalidOperationException($"Elasticsearch brand suggest failed: {brandResponse.ElasticsearchServerError?.Error?.Reason}");

        var suggestions = new List<SuggestionItem>();

        var productSuggestions = productResponse.Suggest?.GetCompletion("product-suggest");
        if (productSuggestions is not null)
        {
            foreach (var suggestion in productSuggestions.SelectMany(s => s.Options))
            {
                suggestions.Add(new SuggestionItem
                {
                    Text = suggestion.Text ?? "",
                    Score = (float)(suggestion.Score ?? 0)
                });
            }
        }

        var brandSuggestions = brandResponse.Suggest?.GetCompletion("brand-suggest");
        if (brandSuggestions is not null)
        {
            foreach (var suggestion in brandSuggestions.SelectMany(s => s.Options))
            {
                suggestions.Add(new SuggestionItem
                {
                    Text = suggestion.Text ?? "",
                    Score = (float)(suggestion.Score ?? 0)
                });
            }
        }

        suggestions = suggestions.OrderByDescending(s => s.Score).Take(query.Size).ToList();

        return new SearchSuggestResult { Suggestions = suggestions };
    }

    public async Task IndexProductAsync(ProductDocument document, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(document, ProductIndexDefinition.IndexName, document.Id, ct);

        if (!response.IsValidResponse)
            logger.LogError("Failed to index product {Id}: {Error}", document.Id, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task UpdateProductAsync(Guid productId, ProductDocument document, CancellationToken ct = default)
    {
        var response = await client.UpdateAsync<ProductDocument, ProductDocument>(
            ProductIndexDefinition.IndexName,
            productId,
            u => u.Doc(document).DocAsUpsert(true), ct);

        if (!response.IsValidResponse)
            logger.LogError("Failed to update product {Id}: {Error}", productId, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task DeleteProductAsync(Guid productId, CancellationToken ct = default)
    {
        var response = await client.DeleteAsync(ProductIndexDefinition.IndexName, productId, ct);
        if (!response.IsValidResponse && response.Result != Result.NotFound)
            logger.LogError("Failed to delete product {Id}: {Error}", productId, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task IndexBrandAsync(BrandDocument document, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(document, BrandIndexDefinition.IndexName, document.Id, ct);

        if (!response.IsValidResponse)
            logger.LogError("Failed to index brand {Id}: {Error}", document.Id, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task UpdateBrandAsync(Guid brandId, BrandDocument document, CancellationToken ct = default)
    {
        var response = await client.UpdateAsync<BrandDocument, BrandDocument>(
            BrandIndexDefinition.IndexName,
            brandId,
            u => u.Doc(document).DocAsUpsert(true), ct);

        if (!response.IsValidResponse)
            logger.LogError("Failed to update brand {Id}: {Error}", brandId, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task DeleteBrandAsync(Guid brandId, CancellationToken ct = default)
    {
        var response = await client.DeleteAsync(BrandIndexDefinition.IndexName, brandId, ct);
        if (!response.IsValidResponse && response.Result != Result.NotFound)
            logger.LogError("Failed to delete brand {Id}: {Error}", brandId, response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task LogSearchQueryAsync(SearchLogEntry entry, CancellationToken ct = default)
    {
        var response = await client.IndexAsync(entry, (IndexName)SearchLogIndexDefinition.IndexName, cancellationToken: ct);

        if (!response.IsValidResponse)
            logger.LogWarning("Failed to log search query: {Error}", response.ElasticsearchServerError?.Error?.Reason);
    }

    public async Task<int> BulkIndexProductsAsync(IEnumerable<ProductDocument> documents, CancellationToken ct = default)
    {
        var bulkResponse = await client.BulkAsync(b => b
            .Index(ProductIndexDefinition.IndexName)
            .IndexMany(documents), ct);

        if (bulkResponse.Errors)
        {
            foreach (var item in bulkResponse.ItemsWithErrors)
                logger.LogError("Bulk index product {Id} failed: {Error}", item.Id, item.Error?.Reason);
        }

        return bulkResponse.Items.Count;
    }

    public async Task<int> BulkIndexBrandsAsync(IEnumerable<BrandDocument> documents, CancellationToken ct = default)
    {
        var bulkResponse = await client.BulkAsync(b => b
            .Index(BrandIndexDefinition.IndexName)
            .IndexMany(documents), ct);

        if (bulkResponse.Errors)
        {
            foreach (var item in bulkResponse.ItemsWithErrors)
                logger.LogError("Bulk index brand {Id} failed: {Error}", item.Id, item.Error?.Reason);
        }

        return bulkResponse.Items.Count;
    }

    private static Query BuildSearchQuery(SearchProductsQuery query)
    {
        var mustClauses = new List<Query>();
        var filterClauses = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            mustClauses.Add(new MultiMatchQuery
            {
                Query = query.Q,
                Fields = new[] { "name^3", "name.ngram^2", "shortDescription^2", "longDescription", "details", "sku" },
                Fuzziness = new Fuzziness("AUTO")
            });
        }
        else
        {
            mustClauses.Add(new MatchAllQuery());
        }

        if (query.CategoryId.HasValue)
            filterClauses.Add(new TermQuery(new Field("categoryId")) { Value = FieldValue.String(query.CategoryId.Value.ToString()) });

        if (query.BrandId.HasValue)
            filterClauses.Add(new TermQuery(new Field("brandId")) { Value = FieldValue.String(query.BrandId.Value.ToString()) });

        if (query.MinPrice.HasValue || query.MaxPrice.HasValue)
        {
            var numberRange = new NumberRangeQuery(new Field("listPrice"));
            if (query.MinPrice.HasValue) numberRange.Gte = (double)query.MinPrice.Value;
            if (query.MaxPrice.HasValue) numberRange.Lte = (double)query.MaxPrice.Value;
            filterClauses.Add(numberRange);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
            filterClauses.Add(new TermQuery(new Field("status")) { Value = FieldValue.String(query.Status) });

        if (query.InStock == true)
            filterClauses.Add(new NumberRangeQuery(new Field("stockQuantity")) { Gt = 0 });

        if (!string.IsNullOrWhiteSpace(query.TagIds))
        {
            var tagIds = query.TagIds.Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var tagId in tagIds)
                filterClauses.Add(new TermQuery(new Field("tags")) { Value = FieldValue.String(tagId.Trim()) });
        }

        if (!string.IsNullOrWhiteSpace(query.AttributeFilters))
        {
            try
            {
                var filters = JsonSerializer.Deserialize<Dictionary<string, string>>(query.AttributeFilters);
                if (filters is not null)
                {
                    foreach (var kvp in filters)
                    {
                        filterClauses.Add(new NestedQuery
                        {
                            Path = new Field("variants.attributeValues"),
                            Query = new BoolQuery
                            {
                                Filter = new Query[]
                                {
                                    new TermQuery(new Field("variants.attributeValues.attributeName")) { Value = FieldValue.String(kvp.Key) },
                                    new TermQuery(new Field("variants.attributeValues.optionValue")) { Value = FieldValue.String(kvp.Value) }
                                }
                            }
                        });
                    }
                }
            }
            catch (JsonException) { }
        }

        return new BoolQuery
        {
            Must = mustClauses,
            Filter = filterClauses
        };
    }

    private static Query BuildBrandSearchQuery(SearchBrandsQuery query)
    {
        var mustClauses = new List<Query>();

        if (!string.IsNullOrWhiteSpace(query.Q))
        {
            mustClauses.Add(new MultiMatchQuery
            {
                Query = query.Q,
                Fields = new[] { "name^3", "name.ngram^2", "shortDescription^2", "longDescription" },
                Fuzziness = new Fuzziness("AUTO")
            });
        }
        else
        {
            mustClauses.Add(new MatchAllQuery());
        }

        return new BoolQuery
        {
            Must = mustClauses
        };
    }

    private static List<SortOptions> BuildSort(string? sort) => sort?.ToLowerInvariant() switch
    {
        "price_asc" => [SortOptions.Field(new Field("listPrice"), new FieldSort { Order = SortOrder.Asc })],
        "price_desc" => [SortOptions.Field(new Field("listPrice"), new FieldSort { Order = SortOrder.Desc })],
        "name_asc" => [SortOptions.Field(new Field("name.keyword"), new FieldSort { Order = SortOrder.Asc })],
        "newest" => [SortOptions.Field(new Field("createdAt"), new FieldSort { Order = SortOrder.Desc })],
        _ => [SortOptions.Score(new ScoreSort { Order = SortOrder.Desc })]
    };

    private static void ExtractTermsFacet(AggregateDictionary aggregations, string key, List<FacetItem> items)
    {
        var termsAgg = aggregations.GetStringTerms(key);
        if (termsAgg is not null)
        {
            foreach (var bucket in termsAgg.Buckets)
                items.Add(new FacetItem { Value = bucket.Key.ToString(), Count = (int)bucket.DocCount });
        }
    }

    private static void ExtractPriceRanges(AggregateDictionary aggregations, List<PriceRangeFacet> items)
    {
        var histAgg = aggregations.GetHistogram("price_ranges");
        if (histAgg is not null)
        {
            foreach (var bucket in histAgg.Buckets)
                items.Add(new PriceRangeFacet
                {
                    From = (decimal)bucket.Key,
                    To = (decimal)bucket.Key + 100,
                    Count = (int)bucket.DocCount
                });
        }
    }
}
