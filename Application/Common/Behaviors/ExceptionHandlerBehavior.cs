using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AE.Market.Application.Common.Behaviors
{
    internal class ExceptionHandlerBehavior<TRequest, TResponse>(
        ILogger<ExceptionHandlerBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
    {
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return next();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "un handeled exception occoured");
                throw new ApplicationException(
                    "An error occurred while processing your request.",
                    ex
                );
            }
        }
    }
}
