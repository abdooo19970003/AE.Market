using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
    internal sealed class RefreshCommandHandler(
        IRepository<RefreshToken> tokenRepo,
        IRepository<User> userRepo,
        IJwtService jwt,
        IRefreshTokenService refreshTokenService
    ) : IRequestHandler<RefreshCommand, Result<TokensResponseDto>>
    {
        public async Task<Result<TokensResponseDto>> Handle(
            RefreshCommand request,
            CancellationToken cancellationToken
        )
        {
            Guid tokenId;
            string rawToken;
            try
            {
                (tokenId, rawToken) = refreshTokenService.Decode(request.OldToken);
            }
            catch (ArgumentException)
            {
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenNotFound);
            }

            var tokenHash = refreshTokenService.HashToken(rawToken);

            var spec = new BaseSpecification<RefreshToken>(t => t.Id == tokenId);
            RefreshToken? token = await tokenRepo.GetBySpecWithTrackingAsync(
                spec,
                cancellationToken
            );
            if (token == null)
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenNotFound);

            if (token.TokenHash != tokenHash)
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenNotFound);

            var user = await userRepo.GetByIdWithTrackingAsync(token.UserId, cancellationToken);

            if (token.IsExpired || token.IsDeleted || user is null)
                return Result<TokensResponseDto>.Fail(AuthErrors.TokenExpiredOrRevoked);
            else if (token.ConsumedAt is not null)
            {
                token.AddDomainEvent(
                    new RefreshTokenReusedDomainEvent(token.UserId, token.TokenHash)
                );
                return Result<TokensResponseDto>.Fail(AuthErrors.ReplayAttackDetected);
            }

            var newRefreshTokenString = jwt.GenerateRefreshToken();
            var newTokenHash = refreshTokenService.HashToken(newRefreshTokenString);
            var refresh = user.RotateRefreshToken(tokenHash, newTokenHash, TimeSpan.FromDays(10));
            await tokenRepo.AddAsync(refresh, cancellationToken);
            var accessToken = jwt.AuthenticateUser(user);
            var response = new TokensResponseDto(
                accessToken,
                refreshTokenService.Encode(refresh.Id, newRefreshTokenString),
                user.Id
            );
            return Result<TokensResponseDto>.Success(response);
        }
    }
}
