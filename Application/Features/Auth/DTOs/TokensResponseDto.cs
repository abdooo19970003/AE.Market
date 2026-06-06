using AE.Market.Domain.Aggregates.Auth;

namespace AE.Market.Application.Features.Auth.DTOs
{
    public sealed record TokensResponseDto(string AccessToken, string RefreshToken,Guid? UserId);
}
