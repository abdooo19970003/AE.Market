using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.CreateBrand;
using AE.Market.Application.Features.Catalog.Commands.DeleteBrand;
using AE.Market.Application.Features.Catalog.Commands.UpdateBrand;
using AE.Market.Application.Features.Catalog.Queries.Brands;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class BrandsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetBrands(CancellationToken ct)
    {
        var result = await mediator.Send(new GetBrandsListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetBrandById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetBrandByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateBrands)]
    public async Task<IActionResult> CreateBrand([FromBody] CreateBrandCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateBrands)]
    public async Task<IActionResult> UpdateBrand(Guid id, [FromBody] UpdateBrandCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateBrands)]
    public async Task<IActionResult> DeleteBrand(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteBrandCommand(id), ct);
        return result.ToDeletedActionResult();
    }
}
