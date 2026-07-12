using AE.Market.Domain.Aggregates.Cart;
using AE.Market.Domain.Aggregates.Cart.Events;
using AE.Market.Domain.Exceptions;
using FluentAssertions;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Domain.Tests.Aggregates.Cart;

public sealed class CartTests
{
    [Fact]
    public void CreateForUser_SetsUserIdAndActiveStatus()
    {
        var userId = Guid.NewGuid();
        var cart = CartAggregate.CreateForUser(Guid.NewGuid(), userId);

        cart.UserId.Should().Be(userId);
        cart.SessionId.Should().BeNull();
        cart.Status.Should().Be(CartStatus.Active);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void CreateForGuest_SetsSessionIdAndActiveStatus()
    {
        var sessionId = Guid.NewGuid();
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), sessionId);

        cart.SessionId.Should().Be(sessionId);
        cart.UserId.Should().BeNull();
        cart.Status.Should().Be(CartStatus.Active);
    }

    [Fact]
    public void AddItem_WithValidData_AddsItemAndFiresEvent()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        var variantId = Guid.NewGuid();

        var item = cart.AddItem(Guid.NewGuid(), variantId, 2);

        item.VariantId.Should().Be(variantId);
        item.Quantity.Should().Be(2);
        cart.Items.Should().ContainSingle();
        cart.DomainEvents.Should().ContainSingle(e => e is CartItemAddedDomainEvent);
    }

    [Fact]
    public void AddItem_DuplicateVariant_IncrementsQuantity()
    {
        var sessionId = Guid.NewGuid();
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), sessionId);
        var variantId = Guid.NewGuid();

        cart.AddItem(Guid.NewGuid(), variantId, 2);
        var item = cart.AddItem(Guid.NewGuid(), variantId, 3);

        item.Quantity.Should().Be(5);
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void AddItem_WithInvalidQuantity_ThrowsDomainException()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());

        var act = () => cart.AddItem(Guid.NewGuid(), Guid.NewGuid(), 0);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Cart.Item.InvalidQuantity");
    }

    [Fact]
    public void RemoveItem_RemovesItemAndFiresEvent()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        var variantId = Guid.NewGuid();
        cart.AddItem(Guid.NewGuid(), variantId, 1);
        cart.ClearDomainEvents();

        cart.RemoveItem(variantId);

        cart.Items.Should().BeEmpty();
        cart.DomainEvents.Should().ContainSingle(e => e is CartItemRemovedDomainEvent);
    }

    [Fact]
    public void RemoveItem_NonExistentVariant_ThrowsDomainException()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());

        var act = () => cart.RemoveItem(Guid.NewGuid());

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Cart.Item.NotFound");
    }

    [Fact]
    public void UpdateItemQuantity_UpdatesQuantityAndFiresEvent()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        var variantId = Guid.NewGuid();
        cart.AddItem(Guid.NewGuid(), variantId, 1);
        cart.ClearDomainEvents();

        cart.UpdateItemQuantity(variantId, 5);

        cart.Items.Should().ContainSingle(i => i.Quantity == 5);
        cart.DomainEvents.Should().ContainSingle(e => e is CartItemAddedDomainEvent);
    }

    [Fact]
    public void UpdateItemQuantity_ZeroQuantity_ThrowsDomainException()
    {
        var cart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        var variantId = Guid.NewGuid();
        cart.AddItem(Guid.NewGuid(), variantId, 1);

        var act = () => cart.UpdateItemQuantity(variantId, 0);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Cart.Item.InvalidQuantity");
    }

    [Fact]
    public void MergeFrom_TransfersItemsToUserCart()
    {
        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var userCart = CartAggregate.CreateForUser(Guid.NewGuid(), userId);
        var guestCart = CartAggregate.CreateForGuest(Guid.NewGuid(), sessionId);
        var variantId = Guid.NewGuid();
        guestCart.AddItem(Guid.NewGuid(), variantId, 3);

        userCart.MergeFrom(guestCart);

        userCart.Items.Should().ContainSingle(i => i.VariantId == variantId && i.Quantity == 3);
        guestCart.Status.Should().Be(CartStatus.Merged);
        userCart.DomainEvents.Should().ContainSingle(e => e is CartMergedDomainEvent);
    }

    [Fact]
    public void MergeFrom_EmptyGuestCart_NoChange()
    {
        var userCart = CartAggregate.CreateForUser(Guid.NewGuid(), Guid.NewGuid());
        var guestCart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());

        userCart.MergeFrom(guestCart);

        userCart.Items.Should().BeEmpty();
        guestCart.Status.Should().Be(CartStatus.Merged);
    }

    [Fact]
    public void MergeFrom_AlreadyMergedGuestCart_ThrowsDomainException()
    {
        var userCart = CartAggregate.CreateForUser(Guid.NewGuid(), Guid.NewGuid());
        var guestCart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        guestCart.AddItem(Guid.NewGuid(), Guid.NewGuid(), 1);
        userCart.MergeFrom(guestCart);
        userCart.ClearDomainEvents();

        var secondGuest = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        secondGuest.AddItem(Guid.NewGuid(), Guid.NewGuid(), 1);

        var act = () => userCart.MergeFrom(guestCart);

        act.Should().Throw<DomainException>()
            .Which.Code.Should().Be("Cart.AlreadyMerged");
    }

    [Fact]
    public void MergeFrom_DuplicateVariants_UpsertsQuantities()
    {
        var userCart = CartAggregate.CreateForUser(Guid.NewGuid(), Guid.NewGuid());
        var guestCart = CartAggregate.CreateForGuest(Guid.NewGuid(), Guid.NewGuid());
        var variantId = Guid.NewGuid();
        userCart.AddItem(Guid.NewGuid(), variantId, 2);
        guestCart.AddItem(Guid.NewGuid(), variantId, 3);

        userCart.MergeFrom(guestCart);

        userCart.Items.Should().ContainSingle(i => i.VariantId == variantId && i.Quantity == 5);
    }
}
