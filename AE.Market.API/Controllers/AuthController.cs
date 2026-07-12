using AE.Market.API.Authentication;
using AE.Market.API.Helpers;
using AE.Market.Application.Features.Auth.Commands.CreateProfile;
using AE.Market.Application.Features.Auth.Commands.DeleteUser;
using AE.Market.Application.Features.Auth.Commands.DisableUser;
using AE.Market.Application.Features.Auth.Commands.EnableUser;
using AE.Market.Application.Features.Auth.Commands.GrantPermission;
using AE.Market.Application.Features.Auth.Commands.Login;
using AE.Market.Application.Features.Auth.Commands.Logout;
using AE.Market.Application.Features.Auth.Commands.Refresh;
using AE.Market.Application.Features.Auth.Commands.Register;
using AE.Market.Application.Features.Auth.Commands.RevokePermission;
using AE.Market.Application.Features.Auth.Commands.UpdateProfile;
using AE.Market.Application.Features.Auth.Queries.Me;
using AE.Market.Application.Features.Auth.Queries.UserById;
using AE.Market.Application.Features.Auth.Queries.UsersList;
using AE.Market.Domain.Aggregates.Auth;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public sealed class AuthController(IMediator mediator) : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            [FromBody] RegisterCommand cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await mediator.Send(cmd, cancellationToken);
            return result.ToCreatedActionResult();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(
            LoginCommand cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await mediator.Send(cmd, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh(
            [FromBody] RefreshCommand cmd,
            CancellationToken cancellationToken
        )
        {
            var result = await mediator.Send(cmd, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var result = await mediator.Send(new LogoutCommand(), ct);
            return result.ToActionResult();
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Profile(CancellationToken ct)
        {
            var result = await mediator.Send(new GetMeQuery(), ct);
            return result.ToNotFoundActionResult();
        }

        [HttpPost("profile")]
        [Authorize]
        public async Task<IActionResult> CreateProfile(
            [FromBody] CreateUserProfileCommand cmd,
            CancellationToken ct
        )
        {
            var result = await mediator.Send(cmd, ct);
            return result.ToCreatedActionResult();
        }

        [HttpPut("profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(
            [FromBody] UpdateUserProfileCommand cmd,
            CancellationToken ct
        )
        {
            var result = await mediator.Send(cmd, ct);
            return result.ToActionResult();
        }

        [HttpGet("users")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> GetUsers(CancellationToken ct)
        {
            var result = await mediator.Send(new GetUsersListQuery(), ct);
            return result.ToActionResult();
        }

        [HttpGet("users/{id:guid}")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> GetUserById(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new GetUserByIdQuery(id), ct);
            return result.ToNotFoundActionResult();
        }

        [HttpPost("users/{userId:guid}/permissions")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> GrantPermission(
            Guid userId,
            [FromQuery] string permission,
            CancellationToken ct
        )
        {
            var result = await mediator.Send(new GrantPermissionCommand(userId, permission), ct);
            return result.ToActionResult();
        }

        [HttpDelete("users/{userId:guid}/permissions")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> RevokePermission(
            Guid userId,
            [FromQuery] string permission,
            CancellationToken ct
        )
        {
            var result = await mediator.Send(new RevokePermissionCommand(userId, permission), ct);
            return result.ToDeletedActionResult();
        }

        [HttpPut("users/{id:guid}/disable")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> DisableUser(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new DisableUserCommand(id), ct);
            return result.ToActionResult();
        }

        [HttpPut("users/{id:guid}/enable")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> EnableUser(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new EnableUserCommand(id), ct);
            return result.ToActionResult();
        }

        [HttpDelete("users/{id:guid}")]
        [Authorize]
        [HasPermission(Permission.MutateUsers)]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new DeleteUserCommand(id), ct);
            return result.ToDeletedActionResult();
        }
    }
}
