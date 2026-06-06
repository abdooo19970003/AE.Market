using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Auth.Commands.Refresh
{
        public sealed class GetRefreshTokenByToken(string token) : BaseSpecification<RefreshToken>(t => t.Token == token);
}
