using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Application.Services;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Queries.Me
{
    internal sealed class GetMeQueryHandler(
        IReadRepository<User> userRepo,
        ICurrentUser currentUser,
        ICacheService cache,
        IMapper mapper
    ) : IRequestHandler<GetMeQuery, Result<UserDetailsDto>>
    {
        async Task<Result<UserDetailsDto>> IRequestHandler<GetMeQuery, Result<UserDetailsDto>>.Handle(
            GetMeQuery request,
            CancellationToken cancellationToken
        )
        {
            var details = await cache.GetOrCreateAsync(
                CacheKeys.UserId(currentUser.UserId),
                async () =>
                {
                    var spec = new UserDetailsByIdSpec(currentUser.UserId);
                    var user = await userRepo.FirstOrDefaultAsync(spec, cancellationToken);
                    if (user is null)
                        return null;

                    return mapper.Map<UserDetailsDto>(user);
                },
                TimeSpan.FromMinutes(5),
                cancellationToken: cancellationToken
            );

            if (details is null)
                return Result<UserDetailsDto>.Fail(AuthErrors.UserNotFound);

            return Result<UserDetailsDto>.Success(details);
        }
    }
}
