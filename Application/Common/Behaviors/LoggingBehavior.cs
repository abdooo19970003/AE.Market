using System.Diagnostics;
using AE.Market.Domain.Common.Abstracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AE.Market.Application.Common.Behaviors
{
    internal sealed class LoggingBehavior<TRequest, TResponse>(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse: Result
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
            if(response.IsSuccess){
                logger.LogInformation(
                    $" {DateTime.UtcNow.ToString("u")} | {typeof(TRequest).Name} | {end}ms "
                );
            }
            else
            {
                logger.LogError("Error: {Error} - SubErrors: {errors}",response.Error, response.Errors);
            }
            return response;
        }
    }
}
