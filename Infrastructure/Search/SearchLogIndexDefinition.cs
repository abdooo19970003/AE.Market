using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Mapping;

namespace AE.Market.Infrastructure.Search;

public static class SearchLogIndexDefinition
{
    public const string IndexName = "search-logs";

    public static CreateIndexRequest GetCreateIndexRequest() => new(IndexName)
    {
        Settings = new IndexSettings
        {
            NumberOfShards = 1,
            NumberOfReplicas = 0
        },
        Mappings = new PropertiesMapping
        {
            ["query"] = new TextProperty(),
            ["filters"] = new ObjectProperty(),
            ["resultCount"] = new IntegerNumberProperty(),
            ["latencyMs"] = new IntegerNumberProperty(),
            ["userId"] = new KeywordProperty(),
            ["timestamp"] = new DateProperty()
        }
    };
}
