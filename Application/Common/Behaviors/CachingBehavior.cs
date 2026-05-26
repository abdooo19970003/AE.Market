using System;
using System.Collections.Generic;
using System.Text;
using AE.Market.Application.Common.Interfaces;
using MediatR;

namespace AE.Market.Application.Common.Behaviors
{
    internal class CachingBehavior<TRequest, TResponse>(ICacheService cache)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : ICachedQuery
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            TResponse result = await cache.GetOrCreateAsync(
                request.CacheKey,
                 () => next(cancellationToken),
                request.AbsoluteExpiration ?? TimeSpan.FromMinutes(60),
                request.SlidingExpiration ,
                cancellationToken
            );
            return result;
        }
    }
}
