using System.Linq.Expressions;

namespace AE.Market.Domain.Common.Specifications
{
    public class BaseSpecification<T>(
        Expression<Func<T, bool>>? criteria = null
        ) : ISpecification<T>
    {

        public Expression<Func<T, bool>>? Criteria { get; } = criteria;

        public List<Expression<Func<T, object>>> Includes { get; } = [];

        public Expression<Func<T, object>>? OrderBy { get; private set; }

        public bool IsDescending { get; private set; }

        public int Skip { get; private set; }

        public int Take { get; private set; }

        public bool IsPagingEnabled { get; private set; }

        public ISpecification<T> AddInclude(Expression<Func<T, object>> expression) { Includes.Add(expression);return this; }
        public ISpecification<T> SetOrderBy(Expression<Func<T, object>> expression, bool desc = true)
        {
            OrderBy = expression;
            IsDescending = desc;
            return this;    
        }
        public ISpecification<T> SetPagination(int skip, int take)
        {
            IsPagingEnabled = true;
            Skip = skip;
            Take = take;
            return this;
        }
    }
}
