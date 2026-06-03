using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Auth.Specs
{
    internal class UserByEmailSpec(string email) : BaseSpecification<User>(u => u.Email == email)
    {
        public UserByEmailSpec(string email, bool includeRefreshTokens = false, bool includePermissions = false)
            : this(email)
        {
            if (includeRefreshTokens)
                AddInclude(u => u.RefreshTokens);
            if (includePermissions)
                AddInclude(u => u.Permissions);
        }
        
    }
}
