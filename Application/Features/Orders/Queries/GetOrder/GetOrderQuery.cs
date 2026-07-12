using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Queries.GetOrder;

public sealed record GetOrderQuery(Guid OrderId) : IBaseQuery<OrderDto>;
