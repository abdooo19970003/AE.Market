using AE.Market.API.Helpers;
using AE.Market.Application.Features.Cart.Commands.AddToCart;
using AE.Market.Application.Features.Cart.Commands.MergeGuestCart;
using AE.Market.Application.Features.Cart.Commands.RemoveFromCart;
using AE.Market.Application.Features.Cart.Commands.UpdateCartItemQuantity;
using AE.Market.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/cart")]
[ApiController]
public sealed class CartController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetCart(
        [FromQuery] Guid? sessionId,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var result = await mediator.Send(new GetCartQuery(userId, sessionId), ct);
        return result.ToActionResult();
    }

    [HttpPost("items")]
    public async Task<IActionResult> AddItem(
        [FromBody] AddToCartCommand cmd,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var enriched = cmd with
        {
            UserId = userId,
            SessionId = userId.HasValue ? null : cmd.SessionId
        };
        var result = await mediator.Send(enriched, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPatch("items/{variantId:guid}")]
    public async Task<IActionResult> UpdateItem(
        Guid variantId,
        [FromBody] UpdateCartItemQuantityCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var userId = GetUserId();
        var enriched = cmd with
        {
            UserId = userId,
            SessionId = userId.HasValue ? null : cmd.SessionId
        };
        var result = await mediator.Send(enriched, ct);
        return result.ToActionResult();
    }

    [HttpDelete("items/{variantId:guid}")]
    public async Task<IActionResult> RemoveItem(
        Guid variantId,
        [FromQuery] Guid? sessionId,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var cmd = new RemoveFromCartCommand(userId, sessionId, variantId);
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("merge")]
    [Authorize]
    public async Task<IActionResult> MergeGuestCart(
        [FromBody] MergeGuestCartCommand cmd,
        CancellationToken ct)
    {
        var userId = GetUserId();
        if (userId is null)
            return Unauthorized();

        var enriched = cmd with { UserId = userId.Value };
        var result = await mediator.Send(enriched, ct);
        return result.ToActionResult();
    }

    private Guid? GetUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return claim is not null && Guid.TryParse(claim, out var id) ? id : null;
    }
}
