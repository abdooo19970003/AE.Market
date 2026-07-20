using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.DTOs;
using AE.Market.Application.Features.Pricing.Specs;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Errors;
using AE.Market.Domain.Aggregates.Pricing;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;
using System.Text.Json;

namespace AE.Market.Application.Features.Orders.Commands.PlaceOrder;

internal sealed class PlaceOrderCommandHandler(
    IRepository<Order> orderRepo,
    IRepository<IdempotencyRequest> idempotencyRepo,
    ICartLookup cartLookup,
    IPriceCalculator priceCalculator,
    IReadRepository<Marketplace> marketplaceRepo,
    IProductVariantLookup variantLookup,
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

        var cartResult = await cartLookup.GetCartForOrderAsync(currentUser.UserId, ct);
        if (cartResult is null)
            return Result<OrderDto>.Fail(OrderErrors.EmptyOrder);

        var (cartId, cartItems) = cartResult.Value;

        var marketplace = await marketplaceRepo.FirstOrDefaultAsync(
            new MarketplaceByCodeSpec("global"), ct);
        var marketplaceId = marketplace?.Id ?? Guid.Empty;

        var variantIds = cartItems.Select(i => i.VariantId).ToList();
        var variantInfos = await variantLookup.GetOrderInfoAsync(variantIds, ct);
        var variantInfoMap = variantInfos.ToDictionary(v => v.VariantId);

        var orderId = Guid.NewGuid();
        var orderNumber = GenerateOrderNumber();
        var orderItems = new List<OrderItem>();

        foreach (var cartItem in cartItems)
        {
            if (!variantInfoMap.TryGetValue(cartItem.VariantId, out var info))
                continue;

            var finalPrice = await priceCalculator.CalculateAsync(
                cartItem.VariantId, cartItem.Quantity, marketplaceId, ct);

            var orderItem = OrderItem.Create(
                Guid.NewGuid(),
                orderId,
                cartItem.VariantId,
                info.ProductName,
                info.VariantName,
                info.Sku,
                finalPrice.UnitPrice.Amount,
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

        await cartLookup.ClearCartAsync(cartId, ct);

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
