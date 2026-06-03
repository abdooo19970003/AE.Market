using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Common.Interfaces
{
    public interface IJwtService
    {
        string AuthanticateUser(User user);
        string GenerateRefreshToken();
    }
}
