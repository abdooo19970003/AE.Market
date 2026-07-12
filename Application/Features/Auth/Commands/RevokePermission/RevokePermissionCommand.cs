using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Commands.RevokePermission;

public sealed record RevokePermissionCommand(Guid UserId, string PermissionName) : ICommand;
