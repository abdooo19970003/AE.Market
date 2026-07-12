using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Inventory.Commands.AdjustStock;
using AE.Market.Application.Features.Inventory.Commands.CreateInventoryItem;
using AE.Market.Application.Features.Inventory.Commands.DeleteInventoryItem;
using AE.Market.Application.Features.Inventory.Commands.ReleaseStock;
using AE.Market.Application.Features.Inventory.Commands.ReserveStock;
using AE.Market.Application.Features.Inventory.Commands.SetQuantity;
using AE.Market.Application.Features.Inventory.Commands.UpdateInventoryItem;
using AE.Market.Application.Features.Inventory.Queries.GetAllInventory;
using AE.Market.Application.Features.Inventory.Queries.GetLowStockReport;
using AE.Market.Application.Features.Inventory.Queries.GetStock;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/admin/inventory")]
[ApiController]
[Authorize]
public sealed class InventoryController(IMediator mediator) : ControllerBase
{
    [HttpGet("low-stock")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> GetLowStockReport(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetLowStockReportQuery(page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpGet("{variantId:guid}")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> GetStock(Guid variantId, CancellationToken ct)
    {
        var result = await mediator.Send(new GetStockQuery(variantId), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> CreateInventoryItem(
        [FromBody] CreateInventoryItemCommand cmd,
        CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPost("{variantId:guid}/adjust")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> AdjustStock(
        Guid variantId,
        [FromBody] AdjustStockCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{variantId:guid}/reserve")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ReserveStock(
        Guid variantId,
        [FromBody] ReserveStockCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{variantId:guid}/release")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> ReleaseStock(
        Guid variantId,
        [FromBody] ReleaseStockCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{variantId:guid}/set-quantity")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> SetQuantity(
        Guid variantId,
        [FromBody] SetQuantityCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpGet]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> GetAllInventory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetAllInventoryQuery(page, pageSize), ct);
        return result.ToActionResult();
    }

    [HttpPut("{variantId:guid}")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> UpdateInventoryItem(
        Guid variantId,
        [FromBody] UpdateInventoryItemCommand cmd,
        CancellationToken ct)
    {
        if (variantId != cmd.VariantId)
            return BadRequest("VariantId mismatch");

        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{variantId:guid}")]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> DeleteInventoryItem(Guid variantId, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteInventoryItemCommand(variantId), ct);
        return result.ToActionResult();
    }
}
