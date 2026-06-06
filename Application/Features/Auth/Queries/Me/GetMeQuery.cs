using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Features.Auth.Queries.Me
{
    public sealed record GetMeQuery(Guid UserId) : IBaseQuery<User>;
}
