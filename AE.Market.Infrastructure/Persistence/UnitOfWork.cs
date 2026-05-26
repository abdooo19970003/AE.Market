using AE.Market.Application.Common.Interfaces;

namespace AE.Market.Infrastructure.Persistence
{
    internal class UnitOfWork(AppDbContext db) : IUnitOfWork
    {
        /// Unit Of Work Staff
        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (db.Database.CurrentTransaction is null)
            {
                await db.Database.BeginTransactionAsync(cancellationToken);
            }
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await db.SaveChangesAsync(cancellationToken);
                if (db.Database.CurrentTransaction is not null)
                    await db.Database.CurrentTransaction.CommitAsync(cancellationToken);
            }
            catch
            {
                await RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (db.Database.CurrentTransaction is not null)
                await db.Database.CurrentTransaction.RollbackAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await db.SaveChangesAsync(cancellationToken);
        }
    }
}
