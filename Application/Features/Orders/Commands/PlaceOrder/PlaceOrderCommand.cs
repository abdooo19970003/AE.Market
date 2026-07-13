using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Commands.PlaceOrder;

public sealed record PlaceOrderCommand(
    string IdempotencyKey
) : ICommand<OrderDto>;
