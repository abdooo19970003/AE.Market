using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.RevokePermission
{
    internal sealed class RevokePermissionCommandHandler(
        IRepository<User> userRepo,
        IRepository<UserPermission> permissionRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<RevokePermissionCommand, Result>
    {
        public async Task<Result> Handle(
            RevokePermissionCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result.Fail(AuthErrors.InsufficientPermissions);

            if (!Enum.TryParse<Permission>(request.PermissionName, ignoreCase: true, out var permission))
                return Result.Fail(new Error("Permission.Invalid", $"The permission '{request.PermissionName}' is not valid."));

            var user = await userRepo.GetByIdWithTrackingAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Fail(AuthErrors.UserNotFound);

            var perSpec = new PermissionByUserId(user.Id);
            var permissions = await permissionRepo.ListWithSpecAsync(perSpec, cancellationToken);
            var target = permissions.FirstOrDefault(p => p.Permission == permission);
            if (target == null)
                return Result.Fail(AuthErrors.PermissionNotFound);

            permissionRepo.Delete(target);
            return Result.Success();
        }
    }
}
