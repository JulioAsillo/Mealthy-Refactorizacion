using Mealthy.Api.Pantry.Domain.Model;
using Mealthy.Api.Security.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Pantry.Infraestructure.Persistence;

public class SupplyConfiguration : IEntityTypeConfiguration<Supply>
{
    public void Configure(EntityTypeBuilder<Supply> b)
    {
        b.ToTable("supplies");
        b.HasKey(s => s.Id);

        b.Property(s => s.Name).HasMaxLength(100).IsRequired();
        b.Property(s => s.Quantity).HasPrecision(10, 2).IsRequired();
        b.Property(s => s.Unit).HasMaxLength(20).IsRequired();
        b.Property(s => s.ExpiresOn);                       // DateOnly → date

        // La despensa es del usuario: si se borra el usuario, se borra su despensa
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        b.HasIndex(s => s.UserId);
    }
}
