using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Media.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Marketplace.Infraestructure.Persistence;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> b)
    {
        b.ToTable("products");
        b.HasKey(p => p.Id);

        b.Property(p => p.Name).HasMaxLength(120).IsRequired();
        b.Property(p => p.Category).HasMaxLength(50).IsRequired();
        b.Property(p => p.Price).HasPrecision(10, 2).IsRequired();   // numeric(10,2), adiós float
        b.Property(p => p.Unit).HasMaxLength(20).IsRequired();
        b.Property(p => p.Stock).IsRequired();

        b.HasOne<Store>()
            .WithMany()
            .HasForeignKey(p => p.StoreId)
            .OnDelete(DeleteBehavior.Cascade);

        // Si la imagen se borra, el producto queda sin foto (no se borra el producto)
        b.HasOne<Image>()
            .WithMany()
            .HasForeignKey(p => p.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(p => p.StoreId);
        b.HasIndex(p => p.Category);
    }
}
