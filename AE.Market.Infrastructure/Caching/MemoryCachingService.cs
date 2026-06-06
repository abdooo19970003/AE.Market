using AE.Market.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace AE.Market.Infrastructure.Caching
{
    internal sealed class MemoryCachingService(IMemoryCache cache) : ICacheService
    {
        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken)
        {
            cache.TryGetValue(key, out T? value);
            return Task.FromResult(value);
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan absoluteExpiration,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        )
        {
            if (cache.TryGetValue(key, out T? value) && value is not null)
                return value;
            value = await factory.Invoke();
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

        public Task RemoveAsync(string key, CancellationToken cancellationToken)
        {
            cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task SetAsync<T>(
            string key,
            T value,
            TimeSpan absoluteExpiration,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        )
        {
            var options = new MemoryCacheEntryOptions();
                options.AbsoluteExpirationRelativeToNow = absoluteExpiration;
            if (slidingExpiration.HasValue)
                options.SetSlidingExpiration(slidingExpiration.Value);
            cache.Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}
