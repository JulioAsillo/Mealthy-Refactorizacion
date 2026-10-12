using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Security.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Marketplace.Infraestructure.Persistence;

public class StoreConfiguration : IEntityTypeConfiguration<Store>
{
    public void Configure(EntityTypeBuilder<Store> b)
    {
        b.ToTable("stores");
        b.HasKey(s => s.Id);

        b.Property(s => s.Name).HasMaxLength(100).IsRequired();
        b.Property(s => s.Description).HasMaxLength(1000).IsRequired();
        b.Property(s => s.Ruc).HasMaxLength(11).IsFixedLength().IsRequired();
        b.Property(s => s.PhotoUrl).HasMaxLength(500);

        // 1:1 con users: el índice único sobre OwnerId garantiza una tienda por dueño
        b.HasOne<User>()
            .WithOne()
            .HasForeignKey<Store>(s => s.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(s => s.OwnerId).IsUnique();
        b.HasIndex(s => s.Ruc).IsUnique();
    }
}
