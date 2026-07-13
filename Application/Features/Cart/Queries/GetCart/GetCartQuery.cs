using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Queries.GetCart;

public sealed record GetCartQuery(
    Guid? UserId,
    Guid? SessionId
) : IBaseQuery<CartDto>;
