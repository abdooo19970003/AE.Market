namespace AE.Market.Application.Features.Cart;

internal static class CacheKeys
{
    internal static string CartByUser(Guid userId) => $"cart-user-{userId}";
    internal static string CartBySession(Guid sessionId) => $"cart-session-{sessionId}";
}
