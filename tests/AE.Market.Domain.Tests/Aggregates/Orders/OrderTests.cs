using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;

namespace AE.Market.Domain.Tests.Aggregates.Orders;

public sealed class OrderTests
{
    [Fact]
    public void Create_WithItems_SetsPropertiesAndFiresEvent()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), orderId, Guid.NewGuid(), "Product A", "Variant A", "SKU-001", 29.99m, 2),
            OrderItem.Create(Guid.NewGuid(), orderId, Guid.NewGuid(), "Product B", "Variant B", "SKU-002", 49.99m, 1),
        };

        var order = Order.Create(orderId, userId, "ORD-001", items);

        order.Id.Should().Be(orderId);
        order.UserId.Should().Be(userId);
        order.OrderNumber.Should().Be("ORD-001");
        order.Status.Should().Be(OrderStatus.Pending);
        order.Total.Should().Be(29.99m * 2 + 49.99m);
        order.Items.Should().HaveCount(2);
        order.DomainEvents.Should().ContainSingle(e => e is OrderPlacedDomainEvent);
    }

    [Fact]
    public void Create_EmptyItems_ThrowsDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), Guid.NewGuid(), "ORD-001", []);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Orders.Empty");
    }

    [Fact]
    public void Cancel_PendingOrder_SetsCancelledAndFiresEvent()
    {
        var orderId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), orderId, Guid.NewGuid(), "Product", "Variant", "SKU", 10m, 1),
        };
        var order = Order.Create(orderId, Guid.NewGuid(), "ORD-002", items);
        order.ClearDomainEvents();

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
        order.DomainEvents.Should().ContainSingle(e => e is OrderCancelledDomainEvent);
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ThrowsDomainException()
    {
        var orderId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), orderId, Guid.NewGuid(), "Product", "Variant", "SKU", 10m, 1),
        };
        var order = Order.Create(orderId, Guid.NewGuid(), "ORD-003", items);
        order.Cancel();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Orders.AlreadyCancelled");
    }

    [Fact]
    public void Cancel_NonPendingOrder_ThrowsDomainException()
    {
        var orderId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), orderId, Guid.NewGuid(), "Product", "Variant", "SKU", 10m, 1),
        };
        var order = Order.Create(orderId, Guid.NewGuid(), "ORD-004", items);
        order.Cancel();

        var act = () => order.Cancel();

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Orders.AlreadyCancelled");
    }

    [Fact]
    public void OrderPlacedEvent_ContainsCorrectItems()
    {
        var orderId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var items = new List<OrderItem>
        {
            OrderItem.Create(Guid.NewGuid(), orderId, variantId, "Product", "Variant", "SKU", 15m, 3),
        };

        var order = Order.Create(orderId, userId, "ORD-005", items);

        var evt = order.DomainEvents.OfType<OrderPlacedDomainEvent>().Single();
        evt.OrderId.Should().Be(orderId);
        evt.UserId.Should().Be(userId);
        evt.Items.Should().ContainSingle(i => i.VariantId == variantId && i.Quantity == 3);
    }
}
