using AE.Market.Domain.Aggregates.Inventory;
using AE.Market.Domain.Aggregates.Inventory.Events;
using AE.Market.Domain.Common.ValueObjects;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Inventory;

public sealed class InventoryItemTests
{
    private static readonly Guid VariantId = Guid.NewGuid();
    private static readonly Guid WarehouseId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ReturnsInventoryItem()
    {
        var item = InventoryItem.Create(
            Guid.NewGuid(),
            VariantId,
            WarehouseId,
            stockQuantity: 100,
            lowStockThreshold: 10);

        item.VariantId.Should().Be(VariantId);
        item.WarehouseId.Should().Be(WarehouseId);
        item.StockQuantity.Should().Be(100);
        item.ReservedQuantity.Should().Be(0);
        item.AvailableQuantity.Should().Be(100);
        item.LowStockThreshold.Should().Be(10);
    }

    [Fact]
    public void Create_WithNegativeStock_ThrowsArgumentOutOfRangeException()
    {
        var act = () => InventoryItem.Create(
            Guid.NewGuid(),
            VariantId,
            WarehouseId,
            stockQuantity: -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void SetQuantity_UpdatesStockAndFiresEvent()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);

        item.SetQuantity(75);

        item.StockQuantity.Should().Be(75);
        item.DomainEvents.Should().ContainSingle(e => e is StockAdjustedDomainEvent);
    }

    [Fact]
    public void SetQuantity_BelowReserved_ThrowsDomainException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 10);
        item.ReserveStock(5);

        var act = () => item.SetQuantity(2);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void AdjustStock_IncreasesQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);

        item.AdjustStock(25);

        item.StockQuantity.Should().Be(75);
    }

    [Fact]
    public void AdjustStock_DecreasesQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);

        item.AdjustStock(-20);

        item.StockQuantity.Should().Be(30);
    }

    [Fact]
    public void AdjustStock_BelowZero_ThrowsInvalidOperationException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 10);

        var act = () => item.AdjustStock(-15);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReserveStock_IncreasesReservedQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);

        item.ReserveStock(20);

        item.ReservedQuantity.Should().Be(20);
        item.AvailableQuantity.Should().Be(30);
        item.DomainEvents.Should().ContainSingle(e => e is StockReservedDomainEvent);
    }

    [Fact]
    public void ReserveStock_ExceedsAvailable_ThrowsInvalidOperationException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 10);

        var act = () => item.ReserveStock(15);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ReserveStock_ZeroQuantity_ThrowsArgumentOutOfRangeException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 10);

        var act = () => item.ReserveStock(0);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ReleaseStock_DecreasesReservedQuantity()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);
        item.ReserveStock(20);

        item.ReleaseStock(10);

        item.ReservedQuantity.Should().Be(10);
        item.AvailableQuantity.Should().Be(40);
        item.DomainEvents.Should().Contain(e => e is StockReleasedDomainEvent);
    }

    [Fact]
    public void ReleaseStock_ExceedsReserved_ThrowsInvalidOperationException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 50);
        item.ReserveStock(5);

        var act = () => item.ReleaseStock(10);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SetLowStockThreshold_UpdatesThreshold()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId, stockQuantity: 5, lowStockThreshold: 0);

        item.SetLowStockThreshold(10);

        item.LowStockThreshold.Should().Be(10);
        item.DomainEvents.Should().Contain(e => e is LowStockThresholdReachedDomainEvent);
    }

    [Fact]
    public void SetLowStockThreshold_NegativeValue_ThrowsArgumentOutOfRangeException()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId);

        var act = () => item.SetLowStockThreshold(-1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Delete_SetsIsDeleted()
    {
        var item = InventoryItem.Create(Guid.NewGuid(), VariantId, WarehouseId);

        item.Delete();

        item.IsDeleted.Should().BeTrue();
    }
}
