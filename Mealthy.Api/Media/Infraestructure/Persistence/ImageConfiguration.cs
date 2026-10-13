using Mealthy.Api.Media.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Media.Infraestructure.Persistence;

public class ImageConfiguration : IEntityTypeConfiguration<Image>
{
    public void Configure(EntityTypeBuilder<Image> b)
    {
        b.ToTable("images");
        b.HasKey(i => i.Id);

        b.Property(i => i.Key).IsRequired();
        b.Property(i => i.ContentType).HasMaxLength(50).IsRequired();
        b.Property(i => i.DeclaredSizeBytes).IsRequired();
        b.Property(i => i.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(i => i.FailureReason).HasMaxLength(300);

        b.HasIndex(i => i.Key).IsUnique();
        b.HasIndex(i => i.OwnerId);                 // sin FK: ver comentario en Image.OwnerId
        b.HasIndex(i => i.CreatedAt);               // la limpieza filtra por antigüedad

        // Propiedad calculada del dominio, no es columna
        b.Ignore(i => i.IsAttachable);
    }
}
