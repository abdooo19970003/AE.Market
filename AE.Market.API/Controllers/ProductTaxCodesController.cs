using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Catalog.Commands.CreateProductTaxCode;
using AE.Market.Application.Features.Catalog.Commands.DeleteProductTaxCode;
using AE.Market.Application.Features.Catalog.Commands.UpdateProductTaxCode;
using AE.Market.Application.Features.Catalog.Queries.ProductTaxCodes;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/product-tax-codes")]
[ApiController]
public sealed class ProductTaxCodesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetProductTaxCodes(CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductTaxCodesListQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProductTaxCodeById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetProductTaxCodeByIdQuery(id), ct);
        return result.ToNotFoundActionResult();
    }

    [HttpPost]
    [Authorize]
    [HasPermission(Permission.MutateTaxCodes)]
    public async Task<IActionResult> CreateProductTaxCode([FromBody] CreateProductTaxCodeCommand cmd, CancellationToken ct)
    {
        var result = await mediator.Send(cmd, ct);
        return result.ToCreatedActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateTaxCodes)]
    public async Task<IActionResult> UpdateProductTaxCode(Guid id, [FromBody] UpdateProductTaxCodeCommand cmd, CancellationToken ct)
    {
        if (id != cmd.Id)
            return BadRequest("Id mismatch");
        var result = await mediator.Send(cmd, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    [HasPermission(Permission.MutateTaxCodes)]
    public async Task<IActionResult> DeleteProductTaxCode(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new DeleteProductTaxCodeCommand(id), ct);
        return result.ToDeletedActionResult();
    }
}
