using Application.Common.Interfaces;
using Domain.Common;
using Domain.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Persistence.Contexts
{
    public class ApplicationDbContext : DbContext
    {
        private readonly IDateTimeService _datetime;
        private readonly IDomainEventDispatcher _domainEventDispatcher;
        private readonly CurrentUser _user;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            IDateTimeService datetime,
            IDomainEventDispatcher domainEventDispatcher,
            ICurrentUserService currentUserService) : base(options)
        {
            //agregamos para poder seguir los cambios y que Entity se de cuenta cuando hace un SaveAsync
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this._datetime = datetime;
            _domainEventDispatcher = domainEventDispatcher;
            _user = currentUserService.User;
        }




        //Sobrescribimos SaveAsync
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            //Cada vez que guardamos o modificamos le decimos que guarde la fecha
            foreach (var entry in ChangeTracker.Entries<AuditableBaseEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedBy = _user.Id;
                        entry.Entity.CreatedDate = _datetime.Now;
                        break;
                    case EntityState.Modified:
                        entry.Entity.ModifiedBy = _user.Id;
                        entry.Entity.ModifiedDate = _datetime.Now;
                        break;
                    case EntityState.Deleted:
                        entry.State = EntityState.Modified;
                        entry.Entity.DeletedBy = _user.Id;
                        entry.Entity.DeletedDate = _datetime.Now;
                        break;
                }
            }
            //return base.SaveChangesAsync();

            int result = await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            // Ignora lo eventos si el dispatcher no tiene nada.
            if (_domainEventDispatcher == null) return result;

            // Ejecuta los eventos solo si el Save fue exitoso.
            var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToArray();

            await _domainEventDispatcher.DispatchAndClearEvents(entitiesWithEvents);

            return result;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //Aca le decimos que se ejecute la configuracion de cada entidad para la migracion
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}