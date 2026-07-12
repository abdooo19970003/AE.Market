using AE.Market.Application.Common.Interfaces;
using AE.Market.Domain.Common.Abstracts;
using AE.Market.Domain.Common.Specifications;
using Microsoft.EntityFrameworkCore;

namespace AE.Market.Infrastructure.Persistence.Repository
{
    internal sealed class Repository<T>(AppDbContext db) : IRepository<T>, IReadRepository<T>
        where T : BaseEntity
    {
        private readonly IQueryable<T> baseQuery = db.Set<T>();
        private readonly DbSet<T> baseCommand = db.Set<T>();

        public async Task<bool> AnyAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        ) =>
            await SpecificationEvaluator<T>
                .GetQuery(baseQuery, spec)
                .AsNoTracking()
                .AnyAsync(cancellationToken);

        public async Task<int> CountAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        ) =>
            await SpecificationEvaluator<T>
                .GetQuery(baseQuery, spec)
                .AsNoTracking()
                .CountAsync(cancellationToken);

        public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            baseQuery
                .AsNoTracking()
                .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        public async Task<T?> FirstOrDefaultAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        ) =>
            await SpecificationEvaluator<T>
                .GetQuery(baseQuery, spec)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> ListAsync(
            CancellationToken cancellationToken = default
        ) => await baseQuery.AsNoTracking().ToListAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> ListWithSpecAsync(
            ISpecification<T> spec,
            CancellationToken cancellationToken = default
        ) =>
            await SpecificationEvaluator<T>
                .GetQuery(baseQuery, spec)
                .AsNoTracking()
                .ToListAsync(cancellationToken: cancellationToken);

        public void Update(T entity) => baseCommand.Update(entity);

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default) =>
            await baseCommand.AddAsync(entity, cancellationToken);

        public async Task AddRangeAsync(
            IEnumerable<T> entities,
            CancellationToken cancellationToken = default
        ) => await baseCommand.AddRangeAsync(entities, cancellationToken);

        public void Delete(T entity) => baseCommand.Remove(entity);

        public void DeleteRange(IEnumerable<T> entities) => baseCommand.RemoveRange(entities);

        public Task<T?> GetByIdWithTrackingAsync(
            Guid id,
            CancellationToken cancellationToken = default
        ) => baseCommand.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        public async Task<T?> GetBySpecWithTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
       => await SpecificationEvaluator<T>.GetQuery(baseCommand, spec).FirstOrDefaultAsync(cancellationToken);

        public async Task<IReadOnlyList<T>> ListWithSpecTrackingAsync(ISpecification<T> spec, CancellationToken cancellationToken = default)
            => await SpecificationEvaluator<T>.GetQuery(baseCommand, spec).ToListAsync(cancellationToken);
    }
}
