using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Login
{
    internal sealed class LoginCommandHandler(
        IRepository<User> userRepo,
        IRepository<RefreshToken> tokenRepo,
        IPasswordService passwordService,
        IJwtService jwt,
        IRefreshTokenService refreshTokenService
    ) : IRequestHandler<LoginCommand, Result<TokensResponseDto>>
    {
        async Task<Result<TokensResponseDto>> IRequestHandler<
            LoginCommand,
            Result<TokensResponseDto>
        >.Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var spec = new UserByEmailSpec(request.Email, includePermissions: true);
            var existing = await userRepo.GetBySpecWithTrackingAsync(spec, cancellationToken);
            if (existing is null)
                return Result<TokensResponseDto>.Fail(AuthErrors.InvalidCredentials);
            if (!existing.IsActive)
                return Result<TokensResponseDto>.Fail(AuthErrors.UserDisabled);
            var isMatch = passwordService.VerifyPassword(request.Password, existing.PasswordHash);
            if (!isMatch)
                return Result<TokensResponseDto>.Fail(AuthErrors.InvalidCredentials);

            var refreshTokenString = jwt.GenerateRefreshToken();
            var tokenHash = refreshTokenService.HashToken(refreshTokenString);
            var refresh = existing.AddRefreshToken(tokenHash, TimeSpan.FromDays(10));
            await tokenRepo.AddAsync(refresh, cancellationToken);
            var accessToken = jwt.AuthenticateUser(existing);
            existing.AddDomainEvent(new UserLoggedInDomainEvent(existing.Id));
            var response = new TokensResponseDto(accessToken, refreshTokenService.Encode(refresh.Id, refreshTokenString), existing.Id);
            return Result<TokensResponseDto>.Success(response);
        }
    }
}