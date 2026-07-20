using AE.Market.API.Helpers;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Orders.Commands.CancelOrder;
using AE.Market.Application.Features.Orders.Commands.PlaceOrder;
using AE.Market.Application.Features.Orders.Queries.GetOrder;
using AE.Market.Application.Features.Orders.Queries.GetOrderHistory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers;

[Route("api/orders")]
[ApiController]
[Authorize]
public sealed class OrdersController(IMediator mediator, ICurrentUser currentUser) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceOrder(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        CancellationToken ct)
    {
        var result = await mediator.Send(new PlaceOrderCommand(idempotencyKey), ct);
        return result.ToCreatedActionResult();
    }

    [HttpGet]
    public async Task<IActionResult> GetHistory(CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderHistoryQuery(currentUser.UserId), ct);
        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetOrderQuery(id), ct);
        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new CancelOrderCommand(id), ct);
        return result.ToActionResult();
    }
}
