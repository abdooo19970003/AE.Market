using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence.Repository
{
    internal static class SpecificationEvaluator<T>
        where T : BaseEntity
    {
        public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
        {
            var query = inputQuery;
            if (spec.Criteria is not null)
                query = query.Where(spec.Criteria);

            query = spec.Includes.Aggregate(query, (q, i) => q.Include(i));

            if (spec.OrderBy is not null)
                query = spec.IsDescending
                    ? query.OrderByDescending(spec.OrderBy)
                    : query.OrderBy(spec.OrderBy);
            if (spec.IsPagingEnabled)
                query = query.Skip(spec.Skip).Take(spec.Take);

            return query;
        }
    }
}
