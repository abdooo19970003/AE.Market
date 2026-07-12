using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.EnableUser
{
    internal sealed class EnableUserCommandHandler(
        IRepository<User> userRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<EnableUserCommand, Result>
    {
        public async Task<Result> Handle(
            EnableUserCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result.Fail(AuthErrors.InsufficientPermissions);

            var user = await userRepo.GetByIdWithTrackingAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Fail(AuthErrors.UserNotFound);

            user.Enable();
            return Result.Success();
        }
    }
}
