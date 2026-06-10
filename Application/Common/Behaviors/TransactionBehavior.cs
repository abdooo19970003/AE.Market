using AE.Market.Application.Common.Abstracts;
using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Common;
using AE.Market.Domain.Exceptions;
using MediatR;
using System.Data;
using System.Reflection;

namespace AE.Market.Application.Common.Behaviors
{
    internal sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IBaseCommand
    {
        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                TResponse response = await next(cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
                return response;
            }
            catch (Exception ex)
            {
                 await unitOfWork.RollbackTransactionAsync(cancellationToken);
                Error error = ex switch
                {
                    DomainException de => new(de.Code, de.Message),
                    _ => ApplicationErrors.ApplicationError(nameof(TransactionBehavior<,>), ex.Message),
                };
                var errorList = new List<Error> { new(ex.Source ?? "TransactionBehavior", ex.Message) };
                if (typeof(TResponse).IsGenericType)
                {
                    var resultType = typeof(TResponse).GetGenericArguments()[0];
                    var resultClosedType = typeof(Result<>).MakeGenericType(resultType);
                    var failMethod = resultClosedType
                        .GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
                        .First(m => m.Name == nameof(Result.Fail) && m.GetParameters().Length == 2);
                    return (TResponse)
                        failMethod.Invoke(null, [error, errorList])!;
                }
                else
                    return (TResponse)
                        (object)Result.Fail(error, errorList);
            }
        }
    }
}
