using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.CreateUnit;
using AE.Market.Application.Features.Catalog.Commands.DeleteUnit;
using AE.Market.Application.Features.Catalog.Commands.SetBaseUnit;
using AE.Market.Application.Features.Catalog.Commands.UpdateUnit;
using AE.Market.Application.Features.Catalog.Queries.Units;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class UnitsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUnits(CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnitsListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetUnitById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetUnitByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateUnits)]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateUnits)]
    public async Task<IActionResult> UpdateUnit(Guid id, [FromBody] UpdateUnitCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id:guid}/set-base")]
    [Authorize]
    [HasPermission(Permission.MutateUnits)]
    public async Task<IActionResult> SetBaseUnit(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new SetBaseUnitCommand(id), ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateUnits)]
    public async Task<IActionResult> DeleteUnit(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteUnitCommand(id), ct);
        return result.ToDeletedActionResult();
    }
}
