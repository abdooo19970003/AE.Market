using AE.Market.Domain.Aggregates.Auth;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Features.Auth.Specs
{
    public sealed class PermissionByUserId(Guid userId) : BaseSpecification<UserPermission>(p => p.UserId == userId);
}
