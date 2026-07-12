using System;
using System.Threading;
using System.Threading.Tasks;
using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Services;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Common.Behaviors
{
    internal sealed class CachingBehavior<TRequest, TResponse>(ICacheService cache)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICachedQuery
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            TResponse? cached = await cache.GetAsync<TResponse>(
                request.CacheKey,
                cancellationToken
            );
            if (cached is not null)
                return cached;

            var result = await next(cancellationToken);

            if (result is Result { IsSuccess: false })
                return result;

            await cache.SetAsync(
                request.CacheKey,
                result,
                request.AbsoluteExpiration ?? TimeSpan.FromMinutes(60),
                request.SlidingExpiration,
                cancellationToken
            );

            return result;
        }
    }
}
