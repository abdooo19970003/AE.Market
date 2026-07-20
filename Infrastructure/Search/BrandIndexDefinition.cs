using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Mapping;

namespace AE.Market.Infrastructure.Search;

public static class BrandIndexDefinition
{
    public const string IndexName = "brands";

    public static CreateIndexRequest GetCreateIndexRequest() => new(IndexName)
    {
        Settings = new IndexSettings
        {
            NumberOfShards = 1,
            NumberOfReplicas = 0,
            Analysis = new Analysis
            {
                Analyzers = new Analyzers
                {
                    ["ngram_suggest"] = new CustomAnalyzer
                    {
                        Tokenizer = "standard",
                        Filter = ["lowercase", "ngram_filter"]
                    }
                },
                TokenFilters = new TokenFilters
                {
                    ["ngram_filter"] = new NGramTokenFilter
                    {
                        MinGram = 2,
                        MaxGram = 10
                    }
                }
            }
        },
        Mappings = new PropertiesMapping
        {
            ["id"] = new KeywordProperty(),
            ["name"] = new TextProperty
            {
                analyzer = "standard",
                Fields = new Properties
                {
                    ["suggest"] = new CompletionProperty
                    {
                        analyzer = "ngram_suggest",
                        SearchAnalyzer = "standard"
                    },
                    ["ngram"] = new TextProperty
                    {
                        analyzer = "ngram_suggest"
                    }
                }
            },
            ["slug"] = new KeywordProperty(),
            ["shortDescription"] = new TextProperty(),
            ["longDescription"] = new TextProperty(),
            ["logoUrl"] = new KeywordProperty(),
            ["isActive"] = new BooleanProperty(),
            ["productCount"] = new IntegerNumberProperty()
        }
    };
}
