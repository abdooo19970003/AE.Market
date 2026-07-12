using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Orders.DTOs;

namespace AE.Market.Application.Features.Orders.Queries.GetOrderHistory;

public sealed record GetOrderHistoryQuery : IBaseQuery<List<OrderDto>>;
