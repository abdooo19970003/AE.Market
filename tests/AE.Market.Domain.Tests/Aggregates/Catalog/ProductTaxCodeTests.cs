using AE.Market.Domain.Aggregates.Catalog.Events;
using AE.Market.Domain.Aggregates.Catalog.Products;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Catalog;

public sealed class ProductTaxCodeTests
{
    private static ProductTaxCode CreateValidTaxCode()
    {
        return ProductTaxCode.Create(Guid.NewGuid(), "txcd_34021000", "physical", null, "Mobile Phones", "Mobile phones");
    }

    public sealed class Create
    {
        [Fact]
        public void Create_WithValidData_ReturnsTaxCode()
        {
            var id = Guid.NewGuid();
            var taxCode = ProductTaxCode.Create(id, "txcd_34021000", "physical", "EU", "Mobile Phones", "Mobile phones");

            taxCode.Id.Should().Be(id);
            taxCode.Code.Should().Be("txcd_34021000");
            taxCode.Type.Should().Be("physical");
            taxCode.PerformanceLocationRequirement.Should().Be("EU");
            taxCode.Name.Should().Be("Mobile Phones");
            taxCode.Description.Should().Be("Mobile phones");
        }

        [Fact]
        public void Create_RaisesProductTaxCodeCreatedDomainEvent()
        {
            var taxCode = CreateValidTaxCode();

            taxCode.DomainEvents.Should().ContainSingle(e => e is ProductTaxCodeCreatedDomainEvent)
                .Which.As<ProductTaxCodeCreatedDomainEvent>().TaxCodeId.Should().Be(taxCode.Id);
        }
    }

    public sealed class UpdateDetails
    {
        [Fact]
        public void UpdateDetails_UpdatesAllFields()
        {
            var taxCode = CreateValidTaxCode();

            taxCode.UpdateDetails("txcd_99999999", "digital", "US", "Digital Goods", "General digital goods");

            taxCode.Code.Should().Be("txcd_99999999");
            taxCode.Type.Should().Be("digital");
            taxCode.PerformanceLocationRequirement.Should().Be("US");
            taxCode.Name.Should().Be("Digital Goods");
            taxCode.Description.Should().Be("General digital goods");
        }

        [Fact]
        public void UpdateDetails_RaisesProductTaxCodeUpdatedDomainEvent()
        {
            var taxCode = CreateValidTaxCode();

            taxCode.UpdateDetails("txcd_99999999", "digital", null, "Digital Goods", "Description");

            taxCode.DomainEvents.Should().Contain(e => e is ProductTaxCodeUpdatedDomainEvent);
        }
    }

    public sealed class DeleteRestore
    {
        [Fact]
        public void Delete_SoftDeletes()
        {
            var taxCode = CreateValidTaxCode();

            taxCode.Delete();

            taxCode.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_RaisesProductTaxCodeDeletedDomainEvent()
        {
            var taxCode = CreateValidTaxCode();

            taxCode.Delete();

            taxCode.DomainEvents.Should().Contain(e => e is ProductTaxCodeDeletedDomainEvent);
        }

        [Fact]
        public void Restore_RecoversSoftDeleted()
        {
            var taxCode = CreateValidTaxCode();
            taxCode.Delete();

            taxCode.Restore();

            taxCode.IsDeleted.Should().BeFalse();
        }
    }
}
