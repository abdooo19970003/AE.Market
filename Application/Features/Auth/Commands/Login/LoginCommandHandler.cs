using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.Events;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Login
{
    internal sealed class LoginCommandHandler(
        IRepository<User> userRepo,
        IRepository<RefreshToken> tokenRepo,
        IPasswordService passwordService,
        IJwtService jwt
    ) : IRequestHandler<LoginCommand, Result<TokensResponseDto>>
    {
        async Task<Result<TokensResponseDto>> IRequestHandler<
            LoginCommand,
            Result<TokensResponseDto>
        >.Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var spec = new UserByEmailSpec(request.Email,includePermissions: true);
            var existing = await userRepo.GetBySpecWithTrackingAsync(spec, cancellationToken);
            if (existing is null)
                return Result<TokensResponseDto>.Fail(AuthErrors.UserNotFound);
            var isMatch = passwordService.VerifyPassword(request.Password, existing.PasswordHash);
            if (!isMatch)
                return Result<TokensResponseDto>.Fail(AuthErrors.UserNotFound);
            var refreshTokenString = jwt.GenerateRefreshToken();
             var refresh =  existing.AddRefreshToken(refreshTokenString, TimeSpan.FromDays(10));
           await tokenRepo.AddAsync(refresh, cancellationToken);
            var accessToken = jwt.AuthanticateUser(existing);
            existing.AddDomainEvent(new UserLoggedInDomainEvent(existing.Id));
            var response = new TokensResponseDto(accessToken, refreshTokenString, existing.Id);
            return Result<TokensResponseDto>.Success(response);

        }
    }
}
