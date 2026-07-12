using AE.Market.Domain.Aggregates.Catalog.Events;
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

    public sealed class UpdateSlug
    {
        [Fact]
        public void UpdateSlug_UpdatesSlug()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.UpdateSlug("mi");

            brand.Slug.Value.Should().Be("mi");
        }

        [Fact]
        public void UpdateSlug_RaisesBrandSlugChangedDomainEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.UpdateSlug("mi");

            brand.DomainEvents.Should().Contain(e => e is BrandSlugChangedDomainEvent);
            var evt = brand.DomainEvents.OfType<BrandSlugChangedDomainEvent>().Single();
            evt.BrandId.Should().Be(brand.Id);
            evt.OldSlug.Should().Be("xiaomi");
            evt.NewSlug.Should().Be("mi");
        }
    }

    public sealed class DeleteRestore
    {
        [Fact]
        public void Delete_SoftDeletes()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.Delete();

            brand.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Delete_RaisesBrandDeletedDomainEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.Delete();

            brand.DomainEvents.Should().Contain(e => e is BrandDeletedDomainEvent);
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_DoesNothing()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
            brand.Delete();
            brand.ClearDomainEvents();

            brand.Delete();

            brand.DomainEvents.Should().NotContain(e => e is BrandDeletedDomainEvent);
        }
    }

    public sealed class MetaFields
    {
        [Fact]
        public void SetOrUpdateMetaFields_SetsValuesAndRaisesEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");

            brand.SetOrUpdateMetaFields("Meta Title", "Meta Desc", "kw1,kw2");

            brand.MetaTitle.Should().Be("Meta Title");
            brand.MetaDescription.Should().Be("Meta Desc");
            brand.MetaKeywords.Should().Be("kw1,kw2");
            brand.DomainEvents.Should().Contain(e => e is BrandMetaFieldsUpdatedDomainEvent);
            var evt = brand.DomainEvents.OfType<BrandMetaFieldsUpdatedDomainEvent>().Single();
            evt.BrandId.Should().Be(brand.Id);
            evt.MetaTitle.Should().Be("Meta Title");
            evt.MetaDescription.Should().Be("Meta Desc");
            evt.MetaKeywords.Should().Be("kw1,kw2");
        }
    }

    public sealed class EventRaised
    {
        [Fact]
        public void UpdateDetails_RaisesBrandDetailsUpdatedDomainEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
            brand.ClearDomainEvents();

            brand.UpdateDetails("Xiaomi Inc.", null, null, null, null, 0);

            brand.DomainEvents.Should().Contain(e => e is BrandDetailsUpdatedDomainEvent);
            var evt = brand.DomainEvents.OfType<BrandDetailsUpdatedDomainEvent>().Single();
            evt.BrandId.Should().Be(brand.Id);
            evt.Name.Should().Be("Xiaomi Inc.");
        }

        [Fact]
        public void Deactivate_RaisesBrandDeactivatedDomainEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
            brand.ClearDomainEvents();

            brand.Deactivate();

            brand.DomainEvents.Should().Contain(e => e is BrandDeactivatedDomainEvent);
            var evt = brand.DomainEvents.OfType<BrandDeactivatedDomainEvent>().Single();
            evt.BrandId.Should().Be(brand.Id);
        }

        [Fact]
        public void Activate_RaisesBrandActivatedDomainEvent()
        {
            var brand = Brand.Create(Guid.NewGuid(), "Xiaomi", "xiaomi");
            brand.Deactivate();
            brand.ClearDomainEvents();

            brand.Activate();

            brand.DomainEvents.Should().Contain(e => e is BrandActivatedDomainEvent);
            var evt = brand.DomainEvents.OfType<BrandActivatedDomainEvent>().Single();
            evt.BrandId.Should().Be(brand.Id);
        }
    }
}
