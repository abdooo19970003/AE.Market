using AE.Market.Domain.Aggregates.Cart.Errors;
using AE.Market.Domain.Aggregates.Cart.Events;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;

namespace AE.Market.Domain.Aggregates.Cart;

public sealed class Cart : BaseEntity, IAggregateRoot
{
    private readonly List<CartItem> _items = [];

    public Guid? UserId { get; private set; }
    public Guid? SessionId { get; private set; }
    public CartStatus Status { get; private set; }
    public IReadOnlyList<CartItem> Items => _items.AsReadOnly();

    private Cart() { }

    private Cart(Guid id, Guid? userId, Guid? sessionId)
        : base(id)
    {
        UserId = userId;
        SessionId = sessionId;
        Status = CartStatus.Active;
    }

    public static Cart CreateForUser(Guid id, Guid userId)
    {
        return new Cart(id, userId, null);
    }

    public static Cart CreateForGuest(Guid id, Guid sessionId)
    {
        return new Cart(id, null, sessionId);
    }

    public CartItem AddItem(Guid itemId, Guid variantId, int quantity)
    {
        if (quantity < 1 || quantity > 999)
            throw new DomainException(
                CartErrors.InvalidQuantity.Code,
                CartErrors.InvalidQuantity.Message
            );

        var existing = _items.Find(i => i.VariantId == variantId);
        if (existing is not null)
        {
            existing.UpdateQuantity(existing.Quantity + quantity);
            AddDomainEvent(new CartItemAddedDomainEvent(Id, variantId, existing.Quantity));
            return existing;
        }

        var item = CartItem.Create(itemId, variantId, quantity);
        _items.Add(item);
        UpdateLastModified();
        AddDomainEvent(new CartItemAddedDomainEvent(Id, variantId, quantity));
        return item;
    }

    public void RemoveItem(Guid variantId)
    {
        var item = _items.Find(i => i.VariantId == variantId) ?? throw new DomainException(
                CartErrors.CartItemNotFound.Code,
                CartErrors.CartItemNotFound.Message
            );
        _items.Remove(item);
        UpdateLastModified();
        AddDomainEvent(new CartItemRemovedDomainEvent(Id, variantId));
    }

    public void UpdateItemQuantity(Guid variantId, int quantity)
    {
        if (quantity < 1 || quantity > 999)
            throw new DomainException(
                CartErrors.InvalidQuantity.Code,
                CartErrors.InvalidQuantity.Message
            );

        var item = _items.Find(i => i.VariantId == variantId) ?? throw new DomainException(
                CartErrors.CartItemNotFound.Code,
                CartErrors.CartItemNotFound.Message
            );
        item.UpdateQuantity(quantity);
        UpdateLastModified();
        AddDomainEvent(new CartItemAddedDomainEvent(Id, variantId, quantity));
    }

    public void MergeFrom(Cart guestCart)
    {
        if (guestCart.Status == CartStatus.Merged)
            throw new DomainException(
                CartErrors.CartAlreadyMerged.Code,
                CartErrors.CartAlreadyMerged.Message
            );

        foreach (var guestItem in guestCart.Items)
        {
            var existing = _items.Find(i => i.VariantId == guestItem.VariantId);
            if (existing is not null)
            {
                existing.UpdateQuantity(existing.Quantity + guestItem.Quantity);
            }
            else
            {
                var item = CartItem.Create(Guid.NewGuid(), guestItem.VariantId, guestItem.Quantity);
                _items.Add(item);
            }
        }

        guestCart.MarkAsMerged();
        UpdateLastModified();
        AddDomainEvent(new CartMergedDomainEvent(
            UserId!.Value,
            guestCart.Id,
            guestCart.Items.Count));
    }

    public void ClearCart()
    {
        _items.Clear();
        Status = CartStatus.Completed;
        UpdateLastModified();
    }

    private void MarkAsMerged()
    {
        Status = CartStatus.Merged;
        UpdateLastModified();
    }
}
