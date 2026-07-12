using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Exceptions;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace AE.Market.Application.Common.Behaviors
{
    internal sealed class ExceptionHandlerBehavior<TRequest, TResponse>(
        ILogger<ExceptionHandlerBehavior<TRequest, TResponse>> logger
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            try
            {
                return await next(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Unhandled exception occurred: {ExceptionType}, {ErrorMessage}",
                    ex.GetType().Name,
                    ex.Message
                );

                Error error = ex switch
                {
                    DomainException de => new(de.Code, de.Message),
                    ValidationException => ApplicationErrors.ValidationError,
                    _ => ApplicationErrors.ApplicationError(ex.GetType().Name, ex.Message)
                };

                if (typeof(TResponse) == typeof(Result))
                    return (TResponse)(object)Result.Fail(error);

                if (
                    typeof(TResponse).IsGenericType
                    && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>)
                )
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var resultClosedType = typeof(Result<>).MakeGenericType(resultType);
                    var failMethod = resultClosedType
                        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .First(m => m.Name == nameof(Result.Fail) && m.GetParameters().Length == 1);
                    return (TResponse)failMethod.Invoke(null, [error])!;
                }

                throw;
            }
        }
    }
}
