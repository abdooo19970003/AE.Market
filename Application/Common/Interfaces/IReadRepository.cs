using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Common.Interfaces
{
    public interface IReadRepository<T> where T: BaseEntity
    {
        // Read Operations - QUERIES
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
    }
}
