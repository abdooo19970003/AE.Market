using AE.Market.API.Options;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Auth;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AE.Market.Infrastructure.Authantication
{
    internal class JwtService(JwtOptions options) : IJwtService
    {
        public string AuthanticateUser(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();


            string permissions = string.Join(separator: ",", user.Permissions.Select(x => x.Permission).ToList());
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = options.Issuer,
                Audience = options.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret)),
                    SecurityAlgorithms.HmacSha256
                ),
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new(ClaimTypes.Email, user.Email.Value),
                    new("Permissions",permissions),
                }),
                Expires = DateTime.UtcNow+TimeSpan.FromMinutes(options.ExpirationInMinutes),
            };

            var securedToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securedToken);
            return accessToken;
        }

        public string GenerateRefreshToken()
        {
            const int length = 32;
            var randomNumber = new byte[length]; 
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
