using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.DTOs;
using AE.Market.Application.Features.Orders.Specs;
using AE.Market.Domain.Aggregates.Orders;
using AE.Market.Domain.Aggregates.Orders.Errors;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using MediatR;

namespace AE.Market.Application.Features.Orders.Commands.CancelOrder;

internal sealed class CancelOrderCommandHandler(
    IRepository<Order> orderRepo,
    ICurrentUser currentUser
) : IRequestHandler<CancelOrderCommand, Result<OrderDto>>
{
    public async Task<Result<OrderDto>> Handle(CancelOrderCommand request, CancellationToken ct)
    {
        var order = await orderRepo.GetBySpecWithTrackingAsync(
            new OrderByIdSpec(request.OrderId), ct);
        if (order is null)
            return Result<OrderDto>.Fail(OrderErrors.OrderNotFound);

        if (order.UserId != currentUser.UserId)
            return Result<OrderDto>.Fail(OrderErrors.OrderNotFound);

        try
        {
            order.Cancel();
        }
        catch (DomainException ex)
        {
            return Result<OrderDto>.Fail(new Error(ex.Code, ex.Message));
        }

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
