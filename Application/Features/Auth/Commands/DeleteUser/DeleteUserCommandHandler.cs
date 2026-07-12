using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.DeleteUser
{
    internal sealed class DeleteUserCommandHandler(
        IRepository<User> userRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<DeleteUserCommand, Result>
    {
        public async Task<Result> Handle(
            DeleteUserCommand request,
            CancellationToken cancellationToken
        )
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result.Fail(AuthErrors.InsufficientPermissions);

            var user = await userRepo.GetByIdWithTrackingAsync(request.UserId, cancellationToken);
            if (user == null)
                return Result.Fail(AuthErrors.UserNotFound);

            user.Delete();
            return Result.Success();
        }
    }
}
