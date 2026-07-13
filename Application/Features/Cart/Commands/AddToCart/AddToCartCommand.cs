using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Commands.AddToCart;

public sealed record AddToCartCommand(
    Guid? UserId,
    Guid? SessionId,
    Guid VariantId,
    int Quantity
) : ICommand<CartDto>;
