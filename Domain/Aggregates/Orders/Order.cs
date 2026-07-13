using AE.Market.Domain.Aggregates.Orders.Errors;
using AE.Market.Domain.Aggregates.Orders.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Orders;

public sealed class Order : BaseEntity, IAggregateRoot
{
    private readonly List<OrderItem> _items = [];

    public string OrderNumber { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public DateTime PlacedAt { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    private Order(Guid id, Guid userId, string orderNumber, List<OrderItem> items, decimal total)
        : base(id)
    {
        UserId = userId;
        OrderNumber = orderNumber;
        _items = items;
        Total = total;
        Status = OrderStatus.Pending;
        PlacedAt = DateTime.UtcNow;
    }

    public static Order Create(Guid id, Guid userId, string orderNumber, List<OrderItem> items)
    {
        if (items.Count == 0)
            throw new DomainException(OrderErrors.EmptyOrder.Code, OrderErrors.EmptyOrder.Message);

        var total = items.Sum(i => i.SellPrice * i.Quantity);
        var order = new Order(id, userId, orderNumber, items, total);

        order.AddDomainEvent(new OrderPlacedDomainEvent(
            order.Id,
            userId,
            items.Select(i => (i.VariantId, i.Quantity)).ToList()));

        return order;
    }

    public void Cancel()
    {
        if (Status == OrderStatus.Cancelled)
            throw new DomainException(OrderErrors.AlreadyCancelled.Code, OrderErrors.AlreadyCancelled.Message);

        if (Status != OrderStatus.Pending)
            throw new DomainException(OrderErrors.NotCancellable.Code, OrderErrors.NotCancellable.Message);

        Status = OrderStatus.Cancelled;
        UpdateLastModified();
        AddDomainEvent(new OrderCancelledDomainEvent(Id, UserId));
    }
}
