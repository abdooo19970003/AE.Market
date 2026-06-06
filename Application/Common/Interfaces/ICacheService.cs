namespace AE.Market.Application.Common.Interfaces
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(
            string key,
            Func<Task<T>> factory,
            TimeSpan absoluteExpiration,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        );
        Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken);
        Task SetAsync<T>(
            string key,
            T value,
            TimeSpan absoluteExpiration ,
            TimeSpan? slidingExpiration = null,
            CancellationToken cancellationToken = default
        );
        Task RemoveAsync(string key, CancellationToken cancellationToken);
    }
}
