using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Pricing.Commands.ActivatePrice;
using AE.Market.Application.Features.Pricing.Commands.DeactivatePrice;
using AE.Market.Application.Features.Pricing.Commands.DeletePrice;
using AE.Market.Application.Features.Pricing.Commands.SetInitialPrice;
using AE.Market.Application.Features.Pricing.Commands.UpdatePrice;
using AE.Market.Application.Features.Pricing.Queries.GetActivePrice;
using AE.Market.Application.Features.Pricing.Queries.GetMargin;
using AE.Market.Application.Features.Pricing.Queries.GetPriceHistory;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Prices;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/products/{productId:guid}/variants/{variantId:guid}/price")]
[ApiController]
public sealed class PricingController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActivePrice(Guid productId, Guid variantId, [FromQuery] Guid? marketplaceId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetActivePriceQuery(variantId, marketplaceId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpGet("history")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> GetPriceHistory(
        Guid productId,
        Guid variantId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetPriceHistoryQuery(variantId, page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpGet("margin")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> GetMargin(Guid productId, Guid variantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetMarginQuery(variantId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetInitialPrice(
        Guid productId,
        Guid variantId,
        [FromBody] SetInitialPriceCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPatch]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> UpdatePrice(
        Guid productId,
        Guid variantId,
        [FromBody] UpdatePriceCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> DeletePrice(
        Guid productId,
        Guid variantId,
        [FromQuery] PriceType type,
        [FromQuery] Guid? marketplaceId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeletePriceCommand(variantId, marketplaceId, type), ct);
        return result.ToActionResult();
    }

    [HttpPost("activate")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ActivatePrice(
        Guid productId,
        Guid variantId,
        [FromBody] ActivatePriceCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("deactivate")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> DeactivatePrice(
        Guid productId,
        Guid variantId,
        [FromQuery] PriceType type,
        [FromQuery] Guid? marketplaceId,
        CancellationToken ct)
    {
        var result = await mediator.Send(new DeactivatePriceCommand(variantId, marketplaceId, type), ct);
        return result.ToActionResult();
    }
}
