using System.Linq.Expressions;

namespace AE.Market.Domain.Common.Specifications
{
    public interface ISpecification<T>
    {
        Expression<Func<T, bool>>? Criteria { get; }
        List<Expression<Func<T,object>>> Includes { get; }
        Expression<Func<T,object>>? OrderBy {  get; }
        bool IsDescending { get; }
        int Skip { get; }
        int Take { get; }
        bool IsPagingEnabled { get; }
    }
}
