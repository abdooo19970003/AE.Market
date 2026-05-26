using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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
    }
}
