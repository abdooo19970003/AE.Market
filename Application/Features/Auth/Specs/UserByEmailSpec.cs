using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Auth.Specs
{
    internal class UserByEmailSpec(string email) : BaseSpecification<User>(u => u.Email == email);
}
