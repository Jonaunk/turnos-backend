using System.Collections;
using Application.Common.Interfaces;
using Domain.Common;
using Persistence.Contexts;

namespace Persistence.Repositories
{
      public class UnitOfWork : IUnitOfWork
  {
      private readonly ApplicationDbContext _dbContext;
      private Hashtable? _repositories;
      private bool disposed;

      public UnitOfWork(ApplicationDbContext dbContext)
      {
          _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
      }

      public IRepositoryAsync<T> Repository<T>() where T : AuditableBaseEntity
      {
          if (_repositories == null)
              _repositories = new Hashtable();

          var type = typeof(T).Name;

          if (!_repositories.ContainsKey(type))
          {
              var repositoryType = typeof(RepositoryAsync<>);

              var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _dbContext);

              _repositories.Add(type, repositoryInstance);
          }

          return (RepositoryAsync<T>)_repositories[type]!;
      }

      public Task Rollback()
      {
          _dbContext.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
          return Task.CompletedTask;
      }

      public async Task<int> Save(CancellationToken cancellationToken)
      {
          return await _dbContext.SaveChangesAsync(cancellationToken);
      }

      public Task<int> SaveAndRemoveCache(CancellationToken cancellationToken, params string[] cacheKeys)
      {
          throw new NotImplementedException();
      }

      public void Dispose()
      {
          Dispose(true);
          GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
          if (disposed)
          {
              if (disposing)
              {
                  //dispose managed resources
                  _dbContext.Dispose();
              }
          }
          //dispose unmanaged resources
          disposed = true;
      }
  }
}
