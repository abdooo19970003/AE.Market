using AE.Market.Domain.Common;
using MediatR;

namespace AE.Market.Application.Common.Abstracts
{
    public interface IBaseQuery<TResponse> : IRequest<Result<TResponse>>;
}
