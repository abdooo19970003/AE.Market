using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Commands.GrantPermission
{
    public sealed record GrantPermissionCommand(Guid UserId, string PermissionName) : ICommand<bool>;
}
