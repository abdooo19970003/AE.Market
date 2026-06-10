namespace AE.Market.Application.Common.Interfaces;

public interface IRefreshTokenService
{
    string HashToken(string rawToken);
    string Encode(Guid tokenId, string rawToken);
    (Guid TokenId, string RawToken) Decode(string encodedToken);
}