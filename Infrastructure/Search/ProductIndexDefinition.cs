using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Analysis;
using Elastic.Clients.Elasticsearch.Mapping;

namespace AE.Market.Infrastructure.Search;

public static class ProductIndexDefinition
{
    public const string IndexName = "products";

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
            ["sku"] = new KeywordProperty(),
            ["shortDescription"] = new TextProperty(),
            ["longDescription"] = new TextProperty(),
            ["details"] = new TextProperty(),
            ["status"] = new KeywordProperty(),
            ["productType"] = new KeywordProperty(),
            ["categoryId"] = new KeywordProperty(),
            ["categoryName"] = new KeywordProperty(),
            ["brandId"] = new KeywordProperty(),
            ["brandName"] = new KeywordProperty(),
            ["tags"] = new KeywordProperty(),
            ["listPrice"] = new FloatNumberProperty(),
            ["stockQuantity"] = new IntegerNumberProperty(),
            ["allowBackOrder"] = new BooleanProperty(),
            ["metaTitle"] = new TextProperty(),
            ["metaDescription"] = new TextProperty(),
            ["metaKeywords"] = new TextProperty(),
            ["createdAt"] = new DateProperty(),
            ["updatedAt"] = new DateProperty(),
            ["variants"] = new NestedProperty
            {
                Properties = new Properties
                {
                    ["id"] = new KeywordProperty(),
                    ["name"] = new TextProperty(),
                    ["sku"] = new KeywordProperty(),
                    ["status"] = new KeywordProperty(),
                    ["listPrice"] = new FloatNumberProperty(),
                    ["stockQuantity"] = new IntegerNumberProperty(),
                    ["attributeValues"] = new NestedProperty
                    {
                        Properties = new Properties
                        {
                            ["attributeId"] = new KeywordProperty(),
                            ["attributeName"] = new KeywordProperty(),
                            ["optionValue"] = new KeywordProperty()
                        }
                    }
                }
            },
            ["images"] = new NestedProperty
            {
                Properties = new Properties
                {
                    ["url"] = new KeywordProperty(),
                    ["altText"] = new TextProperty(),
                    ["sortOrder"] = new IntegerNumberProperty()
                }
            }
        }
    };
}
