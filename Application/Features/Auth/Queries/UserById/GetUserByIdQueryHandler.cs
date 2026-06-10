using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Application.Features.Auth.Specs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Features.Auth.Queries.UserById
{
    internal sealed class GetUserByIdQueryHandler(
        IReadRepository<User> userRepo,
        IMapper mapper,
        ICurrentUser currentUser
    ) : IRequestHandler<GetUserByIdQuery, Result<UserDetailsDto>>
    {
        async Task<Result<UserDetailsDto>> IRequestHandler<
            GetUserByIdQuery,
            Result<UserDetailsDto>
        >.Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result<UserDetailsDto>.Fail(AuthErrors.InsufficientPermissions);

            var spec = new UserDetailsByIdSpec(request.UserId);
            var user = await userRepo.FirstOrDefaultAsync(spec, cancellationToken);
            if (user is null)
                return Result<UserDetailsDto>.Fail(AuthErrors.UserNotFound);

            var dto = mapper.Map<UserDetailsDto>(user);
            return Result<UserDetailsDto>.Success(dto);
        }
    }
}
