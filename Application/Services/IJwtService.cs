using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Services
{
    public interface IJwtService
    {
        string AuthenticateUser(User user);
        string GenerateRefreshToken();
    }
}
