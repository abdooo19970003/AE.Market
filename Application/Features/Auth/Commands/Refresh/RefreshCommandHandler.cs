using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common;
using AE.Market.Domain.Common.DomainErrors;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
    internal partial class RefreshCommandHandler
        (
        IRepository<RefreshToken> tokenRepo,
        IRepository<User> userRepo,
        IJwtService jwt
        )
        : IRequestHandler<RefreshCommand, Result<TokensResponseDto>>
    {
        public async Task<Result<TokensResponseDto>> Handle(RefreshCommand request, CancellationToken cancellationToken)
        {
            var spec = new GetRefreshTokenByToken(request.OldToken);
            RefreshToken? token = await tokenRepo.GetBySpecWithTrackingAsync(spec, cancellationToken);
            if (token == null) {
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenNotFound);
            }

            var user = await userRepo.GetByIdWithTrackingAsync(token.UserId,cancellationToken);

            if (token.IsExpired || token.IsDeleted || user is null)
            {
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenExpiredOrRevoked);
            }
            
            else if(token.ConsumedAt is not null)
            {
                token.AddDominEvent(new RefreshTokenReusedEvent(token.UserId, token.Token));
                return Result<TokensResponseDto>.Fail(AuthErrors.ReplayAttackDetected);
            }
            var newRefreshToken = jwt.GenerateRefreshToken();
            var refresh =  user.RotateRefreshToken(request.OldToken, newRefreshToken, TimeSpan.FromDays(10));
            await tokenRepo.AddAsync(refresh, cancellationToken);
            var accessToken = jwt.AuthanticateUser(user);
            var response = new TokensResponseDto(accessToken, newRefreshToken, user.Id);
            return Result<TokensResponseDto>.Success(response);
        }
    }
}
