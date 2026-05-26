using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AE.Market.Application.Common.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var start = Stopwatch.StartNew();
            TResponse? response = await next(cancellationToken);
            var end = Stopwatch.GetElapsedTime(start.ElapsedMilliseconds);
            logger.LogInformation(
                $" {DateTime.UtcNow.ToString("u")} | {typeof(TRequest).Name} | {end}ms "
            );
            return response;
        }
    }
}
