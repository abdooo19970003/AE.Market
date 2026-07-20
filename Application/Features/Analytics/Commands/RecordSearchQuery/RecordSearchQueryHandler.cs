using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Aggregates.Analytics;
using AE.Market.Domain.Common.Abstracts;
using MediatR;

namespace AE.Market.Application.Features.Analytics.Commands.RecordSearchQuery;

internal sealed class RecordSearchQueryHandler(
    IRepository<SearchAnalytics> repo
) : IRequestHandler<RecordSearchQueryCommand, Result>
{
    public async Task<Result> Handle(RecordSearchQueryCommand request, CancellationToken cancellationToken)
    {
        var entity = SearchAnalytics.Create(
            request.SearchText,
            request.Filters,
            request.ResultCount,
            request.LatencyMs,
            request.UserId);

        await repo.AddAsync(entity, cancellationToken);

        return Result.Success();
    }
}
