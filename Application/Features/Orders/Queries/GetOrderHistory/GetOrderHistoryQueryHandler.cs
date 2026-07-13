using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.DTOs;
using AE.Market.Application.Features.Orders.Specs;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Orders.Queries.GetOrderHistory;

internal sealed class GetOrderHistoryQueryHandler(
    IReadRepository<Order> orderRepo,
    ICurrentUser currentUser
) : IRequestHandler<GetOrderHistoryQuery, Result<List<OrderDto>>>
{
    public async Task<Result<List<OrderDto>>> Handle(GetOrderHistoryQuery request, CancellationToken ct)
    {
        var orders = await orderRepo.ListWithSpecAsync(
            new OrdersByUserIdSpec(currentUser.UserId), ct);

        var dtos = orders.Select(o => new OrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            Status = o.Status.ToString(),
            Total = o.Total,
            PlacedAt = o.PlacedAt,
            Items = o.Items.Select(i => new OrderItemDto
            {
                Id = i.Id,
                VariantId = i.VariantId,
                ProductName = i.ProductName,
                VariantName = i.VariantName,
                Sku = i.Sku,
                SellPrice = i.SellPrice,
                Quantity = i.Quantity,
            }).ToList(),
        }).ToList();

        return Result<List<OrderDto>>.Success(dtos);
    }
}
