using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.DisableUser
{
    internal sealed class DisableUserCommandHandler(
        IRepository<User> userRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<DisableUserCommand, Result>
    {
        public async Task<Result> Handle(
            DisableUserCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result.Fail(AuthErrors.InsufficientPermissions);

            var user = await userRepo.GetByIdWithTrackingAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Fail(AuthErrors.UserNotFound);

            user.Disable();
            return Result.Success();
        }
    }
}
