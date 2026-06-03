using AE.Market.API.Authantication;
using AE.Market.Application.Features.Auth.Commands.GrantPermission;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.Commands.Refresh;
using AE.Market.Application.Features.Auth.Commands.Register;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("/register")]
        public async Task<IActionResult> Register(RegisterCommand cmd)
        {
            var result = await mediator.Send(cmd);
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(result.Errors);
        }

        [HttpPost("/login")]
        public async Task<IActionResult> Login(LoginCommand cmd)
        {
            var result = await mediator.Send(cmd);
            if (result.IsSuccess)
                return Ok(result.Value);
            return BadRequest(result.Error);
        }

        [HttpPost("/refresh")]
        public async Task<IActionResult> Refresh(RefreshCommand cmd)
        {
            var result = await mediator.Send(cmd);
            if (result.IsSuccess)
                return Ok(result);
            return BadRequest(result.Error);
        }

        [HttpGet("/me")]
        [Authorize]
        [HasPermission(Permission.AccessUsers)]
        public async Task<IActionResult> Profile()
        {
            return Ok("I'm In");
        }

        [HttpGet("/users")]
        [Authorize]
        [HasPermission(Permission.AccessUsers)]
        public async Task<IActionResult> GetUsers()
        {
            return Ok("Users List");
        }

        [HttpPut("/grant-permission")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> GrantPermission(GrantPermissionCommand cmd)
        {
            var result = await mediator.Send(cmd); 
            if(!result.IsSuccess)
                return BadRequest(result.Error);
            return Ok(result);
        }
    }
}
