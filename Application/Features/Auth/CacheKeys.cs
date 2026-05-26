namespace AE.Market.Application.Features.Auth
{
    internal static class CacheKeys 
    {
        internal static string UserId(Guid id) => $"user-{id}";
    }
}
