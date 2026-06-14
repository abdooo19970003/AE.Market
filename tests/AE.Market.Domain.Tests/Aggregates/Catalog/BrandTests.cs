using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class BrandTests
{
    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsBrand()
        {
            var id = Guid.NewGuid();
            var brand = Brand.Create(id, "Xiaomi", "xiaomi", "Great phones");

            brand.Id.Should().Be(id);
            brand.Name.Should().Be("Xiaomi");
            brand.Slug.Value.Should().Be("xiaomi");
            brand.ShortDescription.Should().Be("Great phones");
            brand.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Create_ImplementsIAggregateRoot()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.Should().BeAssignableTo<IAggregateRoot>();
        }
    }

    public sealed class UpdateDetails
    {
        [Fact]
        public void UpdateDetails_UpdatesProperties()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.UpdateDetails("Xiaomi Inc.", "New short", "New long", "logo.png", null, 5);

            brand.Name.Should().Be("Xiaomi Inc.");
            brand.ShortDescription.Should().Be("New short");
            brand.LongDescription.Should().Be("New long");
            brand.LogoUrl.Should().Be("logo.png");
            brand.SortOrder.Should().Be(5);
        }
    }

    public sealed class ActivateDeactivate
    {
        [Fact]
        public void Deactivate_SetsIsActiveFalse()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.Deactivate();

            brand.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Activate_SetsIsActiveTrue()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
            brand.Deactivate();

            brand.Activate();

            brand.IsActive.Should().BeTrue();
        }
    }
}
