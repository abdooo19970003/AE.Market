using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Analytics.Queries.GetAdminStats;
using AE.Market.Application.Features.Analytics.Queries.GetTopProducts;
using AE.Market.Application.Features.Analytics.Queries.GetTopSearches;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AE.Market.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
[EnableRateLimiting("admin")]
public sealed class AdminController(IMediator mediator) : ControllerBase
{
    [HttpGet("stats")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAdminStatsQuery(), ct);
        return result.ToActionResult();
    }

    [HttpGet("stats/top-products")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetTopProducts([FromQuery] int days = 30, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTopProductsQuery(days, top), ct);
        return result.ToActionResult();
    }

    [HttpGet("stats/top-searches")]
    [HasPermission(Permission.AccessUsers)]
    public async Task<IActionResult> GetTopSearches([FromQuery] int days = 30, [FromQuery] int top = 10, CancellationToken ct = default)
    {
        var result = await mediator.Send(new GetTopSearchesQuery(days, top), ct);
        return result.ToActionResult();
    }
}
