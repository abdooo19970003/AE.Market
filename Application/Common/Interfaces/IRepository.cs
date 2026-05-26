using AE.Market.Domain.Common;
using AE.Market.Domain.Common.Specifications;

namespace AE.Market.Application.Common.Interfaces
{
    public interface IRepository<T>
        where T : BaseEntity 
    {
        // Read Operations - QUERIES
        Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListAsync(CancellationToken cancellationToken = default);
        Task<IReadOnlyList<T>> ListWithSpecAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<int> CountAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);
        Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken cancellationToken = default);

        // Write Operations - COMMANDS
        Task AddAsync(T entity,  CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default );
        void Delete(T entity);
        void DeleteRange(IEnumerable<T> entities);
        Task<T?> GetByIdWithTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
