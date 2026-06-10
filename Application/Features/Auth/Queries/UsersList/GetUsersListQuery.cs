using AE.Market.Application.Common.Abstracts;

namespace AE.Market.Application.Features.Auth.Queries.UsersList;

public sealed record GetUsersListQuery : IBaseQuery<List<DTOs.UsersListItemDto>>;
