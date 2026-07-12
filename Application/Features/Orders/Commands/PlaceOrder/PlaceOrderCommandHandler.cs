using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Cart.Specs;
using AE.Market.Application.Features.Orders.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Application.Features.Catalog.Products.Specs;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Errors;
using AE.Market.Domain.Aggregates.Prices;
using AE.Market.Domain.Aggregates.Catalog.Products;
using AE.Market.Domain.Aggregates.Catalog.Products.Variants;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;
using System.Text.Json;
using CartAggregate = AE.Market.Domain.Aggregates.Cart.Cart;

namespace AE.Market.Application.Features.Orders.Commands.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<IdempotencyRequest> idempotencyRepo,
    IReadRepository<CartAggregate> cartReadRepo,
    IRepository<CartAggregate> cartRepo,
    IReadRepository<Price> priceRepo,
    IReadRepository<ProductVariant> variantRepo,
    IReadRepository<Product> productRepo,
    ICurrentUser currentUser
) : IRequestHandler<PlaceOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(PlaceOrderCommand request, CancellationToken ct)
    {
        var idempotencyKey = $"place-order-{currentUser.UserId}-{request.IdempotencyKey}";

        var existing = await idempotencyRepo.FirstOrDefaultAsync(
            new IdempotencyByKeySpec(idempotencyKey), ct);
        if (existing is not null)
        {
            var cached = JsonSerializer.Deserialize<OrderDto>(existing.Response,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return Result<OrderDto>.Success(cached!);
        }

        var cartSpec = new CartByUserIdSpec(currentUser.UserId);
        var cart = await cartReadRepo.FirstOrDefaultAsync(cartSpec, ct);
        if (cart is null || cart.Items.Count == 0)
            return Result<OrderDto>.Fail(OrderErrors.EmptyOrder);

        var variantIds = cart.Items.Select(i => i.VariantId).ToList();
        var variants = await variantRepo.ListWithSpecAsync(new VariantsByIdsSpec(variantIds), ct);
        var prices = await priceRepo.ListWithSpecAsync(new ActivePricesByVariantIdsSpec(variantIds), ct);

        var variantMap = variants.ToDictionary(v => v.Id);
        var priceMap = prices.ToDictionary(p => p.VariantId);

        var productIds = variantMap.Values.Select(v => v.ProductId).Distinct().ToList();
        var products = await productRepo.ListWithSpecAsync(new ProductsByIdsSpec(productIds), ct);
        var productMap = products.ToDictionary(p => p.Id);

        var orderId = Guid.NewGuid();
        var orderNumber = GenerateOrderNumber();
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cart.Items)
        {
            if (!variantMap.TryGetValue(cartItem.VariantId, out var variant))
                continue;

            var productName = productMap.TryGetValue(variant.ProductId, out var product)
                ? product.Name
                : string.Empty;

            var sellPrice = priceMap.TryGetValue(cartItem.VariantId, out var price)
                ? price.PriceAmount.Amount
                : 0m;

            var orderItem = OrderItem.Create(
                Guid.NewGuid(),
                orderId,
                cartItem.VariantId,
                productName,
                variant.Name,
                variant.Sku.ToString(),
                sellPrice,
                cartItem.Quantity);

            orderItems.Add(orderItem);
        }

        if (orderItems.Count == 0)
            return Result<OrderDto>.Fail(OrderErrors.EmptyOrder);

        Order order;
        try
        {
            order = Order.Create(orderId, currentUser.UserId, orderNumber, orderItems);
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Fail(new Error("Orders.Domain", ex.Message));
        }

        var trackedCart = await cartRepo.GetBySpecWithTrackingAsync(
            new CartByIdSpec(cart.Id), ct);
        trackedCart?.ClearCart();

        var responseJson = JsonSerializer.Serialize(MapToDto(order));
        await idempotencyRepo.AddAsync(
            new IdempotencyRequest(Guid.NewGuid(), idempotencyKey, responseJson), ct);

        await orderRepo.AddAsync(order, ct);

        return Result<OrderDto>.Success(MapToDto(order));
    }

    private static string GenerateOrderNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = Random.Shared.Next(1000, 9999);
        return $"ORD-{timestamp}-{random}";
    }

    private static OrderDto MapToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            Status = order.Status.ToString(),
            Total = order.Total,
            PlacedAt = order.PlacedAt,
            Items = order.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                Sku = i.Sku,
                SellPrice = i.SellPrice,
                Quantity = i.Quantity,
            }).ToList(),
        };
    }
}
