using AE.Market.Application.Common.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace AE.Market.Application.Services;

internal sealed class RefreshTokenService : IRefreshTokenService
{
    public string HashToken(string rawToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(hash);
    }

    public string Encode(Guid tokenId, string rawToken)
    {
        var encodedId = Convert.ToBase64String(tokenId.ToByteArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
        return $"{encodedId}.{rawToken}";
    }

    public (Guid TokenId, string RawToken) Decode(string encodedToken)
    {
        var parts = encodedToken.Split('.');
        if (parts.Length != 2)
            throw new ArgumentException("Invalid token format.");

        var idPart = parts[0]
            .Replace('-', '+')
            .Replace('_', '/');

        switch (idPart.Length % 4)
        {
            case 2: idPart += "=="; break;
            case 3: idPart += "="; break;
        }

        var idBytes = Convert.FromBase64String(idPart);
        var tokenId = new Guid(idBytes);
        return (tokenId, parts[1]);
    }
}