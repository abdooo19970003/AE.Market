using AE.Market.Application.Services;
using ZiggyCreatures.Caching.Fusion;

namespace AE.Market.Infrastructure.Caching
{
    internal sealed class FusionCachingService(IFusionCache cache) : ICacheService
    {
        public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            return await cache.GetOrDefaultAsync<T>(key,token:cancellationToken);
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan absoluteExpiration,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        )
        {
            T? cachedValue = await cache.GetOrDefaultAsync<T>(key);
            if (cachedValue is not null)
                return cachedValue;
            T? value = await factory();
            if (value is not null)
                await SetAsync(
                    key,
                    value,
                    absoluteExpiration,
                    slidingExpiration,
                    cancellationToken
                );
            return value;
        }

        public async Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            await cache.RemoveAsync(key, token: cancellationToken);
        }

        public async Task SetAsync<T>(
            string key,
            T value,
            TimeSpan absoluteExpiration,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        )
        {
            var options = new FusionCacheEntryOptions { Duration = absoluteExpiration };
            await cache.SetAsync(key, value, options, cancellationToken);
        }
    }
}
