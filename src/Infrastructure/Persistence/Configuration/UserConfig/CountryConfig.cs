using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration.UserConfig
{
    public class CountryConfig : IEntityTypeConfiguration<Country>
    {
        public void Configure(EntityTypeBuilder<Country> builder)
        {
            builder.ToTable("Countries");
            
            builder.HasKey(e => e.Id).HasName("CountryId");

            builder.Property(x => x.Name).HasMaxLength(50);

            builder.HasMany(x => x.Users)
                    .WithOne(x => x.Country)
                    .OnDelete(DeleteBehavior.NoAction); 
        }
    }
}