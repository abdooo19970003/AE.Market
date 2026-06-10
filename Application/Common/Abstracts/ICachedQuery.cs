namespace AE.Market.Application.Common.Abstracts
{
    // Marker for cachable queries...
    internal interface ICachedQuery
    {
        string CacheKey { get; }
        TimeSpan? AbsoluteExpiration { get; }
        TimeSpan? SlidingExpiration { get; }
    }
}
