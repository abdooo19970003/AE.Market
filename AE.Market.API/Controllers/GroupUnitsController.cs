using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.AddUnitToGroup;
using AE.Market.Application.Features.Catalog.Commands.CreateGroupUnit;
using AE.Market.Application.Features.Catalog.Commands.DeleteGroupUnit;
using AE.Market.Application.Features.Catalog.Commands.UpdateGroupUnit;
using AE.Market.Application.Features.Catalog.Queries.GroupUnits;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/group-units")]
[ApiController]
public sealed class GroupUnitsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetGroupUnits(CancellationToken ct)
    {
        var result = await mediator.Send(new GetGroupUnitsListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetGroupUnitById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetGroupUnitByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateGroupUnit([FromBody] CreateGroupUnitCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateGroupUnit(Guid id, [FromBody] UpdateGroupUnitCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/units")]
    [Authorize]
    public async Task<IActionResult> AddUnit(Guid id, [FromBody] AddUnitToGroupCommand cmd, CancellationToken ct)
    {
        if (id != cmd.GroupUnitId)
            return BadRequest("GroupUnitId mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateProducts)]
    public async Task<IActionResult> DeleteGroupUnit(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteGroupUnitCommand(id), ct);
        return result.ToDeletedActionResult();
    }
}
