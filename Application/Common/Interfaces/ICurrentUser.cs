using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Common.Interfaces;

public interface ICurrentUser
{
    Guid UserId { get; }
    string? Email { get; }
    Permission[] Permissions { get; }
    bool IsAuthenticated { get; }
}
