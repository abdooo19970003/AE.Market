using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Logout
{
    public sealed class LogoutCommandHandler(
        IRepository<User> userRepo,
        IRepository<RefreshToken> tokenRepo,
        ICurrentUser currentUser
    ) : IRequestHandler<LogoutCommand, Result>
    {
        public async Task<Result> Handle(
            LogoutCommand request,
            CancellationToken cancellationToken
        )
        {
            var user = await userRepo.GetByIdWithTrackingAsync(
                currentUser.UserId,
                cancellationToken
            );

            if (user is null)
                return Result.Fail(AuthErrors.UserNotFound);

            var tokenSpec = new BaseSpecification<RefreshToken>(t => t.UserId == currentUser.UserId);
            var userTokens = await tokenRepo.ListWithSpecTrackingAsync(tokenSpec, cancellationToken);

            foreach (var token in userTokens)
            {
                token.Delete();
            }

            user.AddDomainEvent(new UserLoggedOutDomainEvent(user.Id));

            return Result.Success();
        }
    }
}
