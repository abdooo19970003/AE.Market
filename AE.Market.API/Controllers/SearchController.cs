using AE.Market.API.Helpers;
using AE.Market.Application.Features.Search.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class SearchController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search([FromQuery] SearchProductsQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return result.ToActionResult();
    }

    [HttpGet("suggest")]
    public async Task<IActionResult> Suggest([FromQuery] SearchSuggestQuery query, CancellationToken ct)
    {
        var result = await mediator.Send(query, ct);
        return result.ToActionResult();
    }
}
