using AE.Market.Domain.Common;

namespace AE.Market.Domain.Aggregates.Auth.Events
{
    public sealed record RefreshTokenReusedDomainEvent(Guid UserId, string UsedToken) : IDomainEvent;

    /// Expected Behavior | Subscribers
    /// 1# Revoke All Tokens for the User (Force Logout) | Already done by User Aggregate
    /// 2# Notify the User of security Issue (Send Email)
    /// 3# Create Log for Analtics 

}
