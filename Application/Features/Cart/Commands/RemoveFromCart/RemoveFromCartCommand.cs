using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Commands.RemoveFromCart;

public sealed record RemoveFromCartCommand(
    Guid? UserId,
    Guid? SessionId,
    Guid VariantId
) : ICommand<CartDto>;
