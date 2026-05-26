using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Login
{
    internal class LoginCommandHandler(
        IRepository<User> repo,
        ICacheService cache,
        IPasswordService passwordService
    ) : IRequestHandler<LoginCommand, Result<TokensResponseDto>>
    {
        async Task<Result<TokensResponseDto>> IRequestHandler<
            LoginCommand,
            Result<TokensResponseDto>
        >.Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var spec = new UserByEmailSpec(request.Email);
            var existing = await repo.FirstOrDefaultAsync(spec, cancellationToken);
            if (existing is null)
                return Result<TokensResponseDto>.Fail(AuthErrors.UserNotFound);
            var isMatch = passwordService.VerifyPassword(request.Password, existing.PasswordHash);
            if (!isMatch)
                return Result<TokensResponseDto>.Fail(AuthErrors.UserNotFound);

            existing.AddRefreshToken("New_RefreshToken", TimeSpan.FromDays(7));

            var response = new TokensResponseDto("Some ACess Token", "New_RefreshToken", existing.Id);
            return Result<TokensResponseDto>.Success(response);

        }
    }
}
