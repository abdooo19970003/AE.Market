using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Common.Abstracts;

public interface ISearchQuery<TResponse> : IRequest<Result<TResponse>> { }
