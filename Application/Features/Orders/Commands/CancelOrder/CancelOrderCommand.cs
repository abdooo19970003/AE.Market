using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Commands.CancelOrder;

public sealed record CancelOrderCommand(Guid OrderId) : ICommand<OrderDto>;
