using AE.Market.Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using System.Reflection;

namespace AE.Market.Application.Common.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(
        IEnumerable<IValidator<TRequest>> validators
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            ArgumentNullException.ThrowIfNull(next);

            if (validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                        validators.Select(v => v.ValidateAsync(context, cancellationToken))
                    )
                    .ConfigureAwait(false);
                var failures = validationResults
                    .Where(r => r.Errors.Count > 0)
                    .SelectMany(r => r.Errors)
                    .Select(f => new Error(f.PropertyName, f.ErrorMessage))
                    .ToList();
                if (failures.Count > 0)
                {
                    if (typeof(TResponse) == typeof(Result))
                        return (TResponse)
                            (object)Result.Fail(ApplicationErrors.ValidationError, failures);
                    if (
                        typeof(TResponse).IsGenericType
                        && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>)
                    )
                    {
                        var resultType = typeof(TResponse).GetGenericArguments()[0];

                        //// Invoke the static Result.Failure<T> method dynamically
                        //var failureMethode = typeof(Result)
                        //    .GetMethods()
                        //    .First(m =>
                        //        m.Name == nameof(Result.Fail)
                        //        && m.IsGenericMethod
                        //        && m.GetParameters().Length == 2
                        //    )
                        //    .MakeGenericMethod(resultType);
                        var resultClosedType = typeof(Result<>).MakeGenericType(resultType);
                        var failureMethode = resultClosedType
                            .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                            .First(m => m.Name == nameof(Result.Fail) && m.GetParameters().Length == 2);
                        return (TResponse)
                            failureMethode.Invoke(
                                null,
                                parameters: [ApplicationErrors.ValidationError, failures]
                            )!;
                    }

                    // fallback
                    throw new ValidationException(
                        failures.Select(f => new ValidationFailure(f.Code, f.Message))
                    );
                }
            }
            return await next(cancellationToken).ConfigureAwait(false);
        }
    }
}
