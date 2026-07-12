using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.GrantPermission
{
    internal sealed class GrantPermissionCommandHandler(
        IRepository<User> userRepo,
        IRepository<UserPermission> permissionRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<GrantPermissionCommand, Result<bool>>
    {
        public async Task<Result<bool>> Handle(
            GrantPermissionCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result<bool>.Fail(AuthErrors.InsufficientPermissions);

            if (!Enum.TryParse<Permission>(request.PermissionName, ignoreCase: true, out var permission))
                return Result<bool>.Fail(new Error("Permission.Invalid", $"The permission '{request.PermissionName}' is not valid."));

            var user = await userRepo.GetByIdWithTrackingAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result<bool>.Fail(AuthErrors.UserNotFound);
            var perSpec = new PermissionByUserId(user.Id);
            var permissions = await permissionRepo.ListWithSpecAsync(perSpec, cancellationToken);
            if (permissions.Any(p => p.Permission == permission))
                return Result<bool>.Fail(new Error("User.Permissions.AlreadyExist", "The User already has the permission you're trying to grant"));

            UserPermission userPermission = user.AddPermission(permission);
            await permissionRepo.AddAsync(userPermission, cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
