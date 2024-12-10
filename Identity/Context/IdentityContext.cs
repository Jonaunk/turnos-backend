using Domain.Common;
using Domain.Common.Interfaces;
using Identity.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Identity.Context
{
    public class IdentityContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IDomainEventDispatcher _domainEventDispatcher;

        public IdentityContext(DbContextOptions<IdentityContext> options, IDomainEventDispatcher domainEventDispatcher) : base(options)
        {
            _domainEventDispatcher = domainEventDispatcher;
        }

        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
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
    }
}
