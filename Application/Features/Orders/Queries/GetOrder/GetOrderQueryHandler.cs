using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.DTOs;
using AE.Market.Application.Features.Orders.Specs;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Orders.Queries.GetOrder;

internal sealed class GetOrderQueryHandler(
    IReadRepository<Order> orderRepo,
    ICurrentUser currentUser
) : IRequestHandler<GetOrderQuery, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(GetOrderQuery request, CancellationToken ct)
    {
        var order = await orderRepo.FirstOrDefaultAsync(
            new OrderByIdSpec(request.OrderId), ct);
        if (order is null)
            return Result<OrderDto>.Fail(OrderErrors.OrderNotFound);

        if (order.UserId != currentUser.UserId)
            return Result<OrderDto>.Fail(OrderErrors.OrderNotFound);

        var dto = new OrderDto
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

        return Result<OrderDto>.Success(dto);
    }
}
