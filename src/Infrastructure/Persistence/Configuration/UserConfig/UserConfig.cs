using Domain.Entities.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Configuration.UserConfig
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id).HasName("UserId");

            builder.Property(a => a.FirstName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(a => a.LastName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(a => a.Gender)
                   .IsRequired();

            builder.Property(a => a.Email)
                   .IsRequired();

            builder.Property(a => a.ProjectSupported)
                   .IsRequired();

            builder.Property(a => a.Amount)
                   .HasColumnType("decimal(18,2)")
                   .IsRequired();

            builder.Property(a => a.PasswordHash)
                   .IsRequired();


            // Configuración de la relación con Country 
            builder.HasOne(a => a.Country)
                   .WithMany(c => c.Users) // Si la relación es uno a muchos
                   .HasForeignKey(a => a.CountryId) // Define la FK si existe
                   .OnDelete(DeleteBehavior.NoAction)
                   .IsRequired();
        }
    }
}