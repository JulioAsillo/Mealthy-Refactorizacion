using Mealthy.Api.Security.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Security.Infraestructure.Persistence;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> b)
    {
        b.ToTable("refresh_tokens");
        b.HasKey(t => t.Id);

        // SHA-256 en Base64Url = 43 caracteres
        b.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        b.Property(t => t.ReplacedByTokenHash).HasMaxLength(64);
        b.Property(t => t.ExpiresAt).IsRequired();
        b.Property(t => t.RevokedAt);

        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(t => t.TokenHash).IsUnique();
        b.HasIndex(t => t.UserId);
    }
}
