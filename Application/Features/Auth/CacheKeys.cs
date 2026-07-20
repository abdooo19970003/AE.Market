namespace AE.Market.Application.Features.Auth;

internal static class CacheKeys
{
    internal static string UserId(Guid id) => $"user-{id}";
    internal static string UsersList(int page, int pageSize) => $"users-list-p{page}s{pageSize}";
}
