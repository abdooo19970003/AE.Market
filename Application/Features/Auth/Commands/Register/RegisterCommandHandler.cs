using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Aggregates.Auth.ValueObjects;
using AE.Market.Domain.Common;
using AE.Market.Domain.Common.DomainErrors;
using AE.Market.Domain.Common.Specifications;
using MediatR;

namespace AE.Market.Application.Features.Auth.Commands.Register
{
    public sealed class RegisterCommandHandler(IRepository<User> repo, IPasswordService passwordService, IJwtService jwt)
        : IRequestHandler<RegisterCommand, Result<TokensResponseDto>>
    {
        public async Task<Result<TokensResponseDto>> Handle(
            RegisterCommand request,
            CancellationToken cancellationToken
        )
        {
            var email = request.Email.Trim().ToLower();

            var existing = await repo.AnyAsync(new UserByEmailSpec(email), cancellationToken);
            if (existing)
                return Result<TokensResponseDto>.Fail(AuthErrors.EmailAlreadyExists);

            var hash = passwordService.HashPassword(request.Password);
            var user = User.Register(Guid.NewGuid(), email, hash);
            var refreshToken = jwt.GenerateRefreshToken();
            var accessToken = jwt.AuthanticateUser(user);
            user.AddRefreshToken(refreshToken, TimeSpan.FromDays(10));
            await repo.AddAsync(user, cancellationToken);
            TokensResponseDto result = new(accessToken, refreshToken, user.Id);
            return Result<TokensResponseDto>.Success(result);
        }
    }
}
