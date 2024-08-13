using Domain.Common;

namespace Application.Common.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepositoryAsync<T> Repository<T>() where T : AuditableBaseEntity;

        Task<int> Save(CancellationToken cancellationToken);

        Task<int> SaveAndRemoveCache(CancellationToken cancellationToken, params string[] cacheKeys);

        Task Rollback();
    }
}