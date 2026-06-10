using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;

namespace AE.Market.Application.Features.Auth.Queries.UserById;

public sealed record GetUserByIdQuery(Guid UserId) : IBaseQuery<UserDetailsDto>;
