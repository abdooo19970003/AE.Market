using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Commands.UpdateCartItemQuantity;

public sealed record UpdateCartItemQuantityCommand(
    Guid? UserId,
    Guid? SessionId,
    Guid VariantId,
    int Quantity
) : ICommand<CartDto>;
