using Application.Common.Interfaces;
using Ardalis.Specification.EntityFrameworkCore;
using Persistence.Contexts;

namespace Persistence.Repositories
{
    public class RepositoryAsync<T> : RepositoryBase<T>, IRepositoryAsync<T> where T : class
    {
        private readonly ApplicationDbContext _dbContext;

        public RepositoryAsync(ApplicationDbContext dbContext) : base(dbContext)
        {
            this._dbContext = dbContext;
        }

    }
}