using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using System.Security.Claims;

namespace AE.Market.API.Authentication;

internal sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private readonly ClaimsPrincipal? _user = httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var claim = _user?.FindFirstValue(ClaimTypes.NameIdentifier);
            return claim is not null ? Guid.Parse(claim) : Guid.Empty;
        }
    }

    public string? Email => _user?.FindFirstValue(ClaimTypes.Email);

    public Permission[] Permissions
    {
        get
        {
            var claim = _user?.FindFirstValue("Permissions");
            if (claim is null) return [];
            return claim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => Enum.Parse<Permission>(p.Trim()))
                .ToArray();
        }
    }

    public bool IsAuthenticated => _user?.Identity?.IsAuthenticated ?? false;

    public bool IsTokenExpiringSoon()
    {
        var expClaim = _user?.FindFirstValue("exp");
        if (expClaim is null) return false;

        if (long.TryParse(expClaim, out var expUnix))
        {
            var expDateTime = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;
            return expDateTime - DateTime.UtcNow < TimeSpan.FromMinutes(5);
        }
        return false;
    }
}
