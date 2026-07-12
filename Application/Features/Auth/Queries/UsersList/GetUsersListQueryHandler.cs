using AE.Market.Application.Common.Interfaces;
using AE.Market.Application.Common.Mapping;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Aggregates.Auth.Errors;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Auth.Queries.UsersList
{
    internal sealed class GetUsersListQueryHandler(
        IReadRepository<User> userRepo,
        IMapper mapper,
        ICurrentUser currentUser
    ) : IRequestHandler<GetUsersListQuery, Result<List<UsersListItemDto>>>
    {
        async Task<Result<List<UsersListItemDto>>> IRequestHandler<
            GetUsersListQuery,
            Result<List<UsersListItemDto>>
        >.Handle(GetUsersListQuery request, CancellationToken cancellationToken)
        {
            if (!currentUser.Permissions.Contains(Permission.MutateUsers))
                return Result<List<UsersListItemDto>>.Fail(AuthErrors.InsufficientPermissions);

            var users = await userRepo.ListAsync(cancellationToken);
            var dto = mapper.Map<List<UsersListItemDto>>(users);
            return Result<List<UsersListItemDto>>.Success(dto);
        }
    }
}
