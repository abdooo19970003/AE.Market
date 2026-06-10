using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Features.Auth.DTOs;
using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Features.Auth.Queries.Me
{
    public sealed record GetMeQuery() : IBaseQuery<UserDetailsDto>;
}
