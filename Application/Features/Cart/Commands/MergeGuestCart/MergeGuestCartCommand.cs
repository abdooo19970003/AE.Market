using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Cart.DTOs;

namespace AE.Market.Application.Features.Cart.Commands.MergeGuestCart;

public sealed record MergeGuestCartCommand(
    Guid UserId,
    Guid SessionId
) : ICommand<CartDto>;
