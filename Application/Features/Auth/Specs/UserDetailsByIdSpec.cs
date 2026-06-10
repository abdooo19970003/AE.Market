using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Auth.Specs
{
    public sealed class UserDetailsByIdSpec : BaseSpecification<User> {
        public UserDetailsByIdSpec(Guid userId) : base(u => u.Id == userId)
        {
            AddInclude(u => u.RefreshTokens);
            AddInclude(u => u.Permissions);
            AddInclude(u => u.Profile);
        }
    }
}
