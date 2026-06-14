using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog.Products;

public sealed class TagTests
{
    [Fact]
    public void Create_WithValidData_SetsNameAndSlug()
    {
        var id = Guid.NewGuid();

        var tag = Tag.Create(id, "Summer Collection", "summer-collection");

        tag.Id.Should().Be(id);
        tag.Name.Should().Be("Summer Collection");
        tag.Slug.Value.Should().Be("summer-collection");
    }
}
