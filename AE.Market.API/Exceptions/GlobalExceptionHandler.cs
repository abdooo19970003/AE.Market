using AE.Market.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AE.Market.API.Exceptions
{
    public sealed class GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetailsService
    ) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken
        )
        {
            logger.LogError(exception, "Unhandled exception occurred: {ExceptionType}, {ErrorMessage}", exception.GetType().Name, exception.Message);

            var (statusCode, title) = exception switch
            {
                DomainException => (StatusCodes.Status400BadRequest, "Bad Request"),
                ValidationException => (StatusCodes.Status400BadRequest, "Validation Failed"),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error")
            };

            httpContext.Response.StatusCode = statusCode;

            return await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    Exception = exception,
                    ProblemDetails = new ProblemDetails
                    {
                        Type = exception.GetType().FullName,
                        Title = title,
                        Detail = exception.Message,
                        Status = statusCode
                    },
                }
            );
        }
    }
}
