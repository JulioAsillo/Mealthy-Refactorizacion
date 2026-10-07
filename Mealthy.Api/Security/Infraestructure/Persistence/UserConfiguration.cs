using Mealthy.Api.Security.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Security.Infraestructure.Persistence;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);

        b.Property(u => u.Username).HasMaxLength(30).IsRequired();
        b.Property(u => u.Email).HasMaxLength(254).IsRequired();
        b.Property(u => u.PasswordHash).HasMaxLength(100).IsRequired();
        b.Property(u => u.FirstName).HasMaxLength(80).IsRequired();
        b.Property(u => u.LastName).HasMaxLength(80).IsRequired();
        b.Property(u => u.Phone).HasMaxLength(20);
        b.Property(u => u.BirthDate);                       // DateOnly → date en Postgres
        b.Property(u => u.Role).HasConversion<string>().HasMaxLength(20).IsRequired();

        b.HasIndex(u => u.Email).IsUnique();
        b.HasIndex(u => u.Username).IsUnique();
    }
}
