using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Features.Auth.Commands.GrantPermission
{
    public sealed record GrantPermissionCommand(Guid UserId, Permission Permission) : ICommand<bool>;
    
}
