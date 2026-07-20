using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Queries.Me;

internal sealed class GetMeQueryHandler(
    IReadRepository<User> userRepo,
    ICurrentUser currentUser,
    IMapper mapper
) : IRequestHandler<GetMeQuery, Result<UserDetailsDto>>
{
    public async Task<Result<UserDetailsDto>> Handle(
        GetMeQuery request,
        CancellationToken cancellationToken
    )
    {
        var spec = new UserDetailsByIdSpec(currentUser.UserId);
        var user = await userRepo.FirstOrDefaultAsync(spec, cancellationToken);
        if (user is null)
            return Result<UserDetailsDto>.Fail(AuthErrors.UserNotFound);

        return Result<UserDetailsDto>.Success(mapper.Map<UserDetailsDto>(user));
    }
}
