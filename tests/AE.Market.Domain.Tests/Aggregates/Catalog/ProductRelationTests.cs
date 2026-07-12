using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class ProductRelationTests
{
    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsRelation()
        {
            var id = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var relatedProductId = Guid.NewGuid();

            var relation = ProductRelation.Create(id, productId, relatedProductId, RelationType.CrossSell, sortOrder: 5);

            relation.Id.Should().Be(id);
            relation.ProductId.Should().Be(productId);
            relation.RelatedProductId.Should().Be(relatedProductId);
            relation.Type.Should().Be(RelationType.CrossSell);
            relation.SortOrder.Should().Be(5);
        }
    }

    public sealed class UpdateSortOrder
    {
        [Fact]
        public void UpdateSortOrder_ChangesSortOrder()
        {
            var relation = ProductRelation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationType.Related, sortOrder: 1);

            relation.UpdateSortOrder(10);

            relation.SortOrder.Should().Be(10);
        }
    }

    public sealed class DeleteRelation
    {
        [Fact]
        public void Delete_SoftDeletes()
        {
            var relation = ProductRelation.Create(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), RelationType.UpSell);

            relation.Delete();

            relation.IsDeleted.Should().BeTrue();
        }
    }
}
