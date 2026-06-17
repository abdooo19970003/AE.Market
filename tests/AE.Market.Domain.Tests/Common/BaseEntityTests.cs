using AE.Market.Domain.Common.Abstracts;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Common;

public sealed class BaseEntityTests
{
    private sealed class TestEntity : BaseEntity
    {
        public TestEntity(Guid id) : base(id)
        {
        }
    }

    [Fact]
    public void Constructor_SetsId()
    {
        var id = Guid.NewGuid();

        var entity = new TestEntity(id);

        entity.Id.Should().Be(id);
    }

    [Fact]
    public void Constructor_SetsCreatedAt()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Delete_SetsIsDeleted()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Delete();

        entity.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void Delete_RaisesEntityDeletedDomainEvent()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Delete();

        entity.DomainEvents.Should().Contain(e => e is EntityDeletedDomainEvent);
    }

    [Fact]
    public void Delete_Idempotent_NoDuplicateEvent()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Delete();
        entity.Delete();

        entity.DomainEvents.Should().ContainSingle(e => e is EntityDeletedDomainEvent);
    }

    [Fact]
    public void Restore_AfterDelete_ClearsIsDeleted()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Delete();

        entity.Restore();

        entity.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Restore_RaisesEntityRestoredDomainEvent()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Delete();

        entity.Restore();

        entity.DomainEvents.Should().Contain(e => e is EntityRestoredDomainEvent);
    }

    [Fact]
    public void ClearDomainEvents_EmptiesList()
    {
        var entity = new TestEntity(Guid.NewGuid());
        entity.Delete();

        entity.ClearDomainEvents();

        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Equals_SameId_ReturnsTrue()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.Equals(b).Should().BeTrue();
    }

    [Fact]
    public void Equals_DifferentId_ReturnsFalse()
    {
        var a = new TestEntity(Guid.NewGuid());
        var b = new TestEntity(Guid.NewGuid());

        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void Equals_Null_ReturnsFalse()
    {
        var entity = new TestEntity(Guid.NewGuid());

        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void GetHashCode_SameId_ReturnsSame()
    {
        var id = Guid.NewGuid();
        var a = new TestEntity(id);
        var b = new TestEntity(id);

        a.GetHashCode().Should().Be(b.GetHashCode());
    }
}
