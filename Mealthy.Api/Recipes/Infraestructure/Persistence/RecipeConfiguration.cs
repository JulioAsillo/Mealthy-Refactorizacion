using Mealthy.Api.Media.Domain.Model;
using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Security.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Recipes.Infraestructure.Persistence;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> b)
    {
        b.ToTable("recipes");
        b.HasKey(r => r.Id);

        b.Property(r => r.Title).HasMaxLength(120).IsRequired();
        b.Property(r => r.Description).HasMaxLength(2000).IsRequired();
        b.Property(r => r.PreparationMinutes).IsRequired();
        b.Property(r => r.Servings).IsRequired();

        // FK real a users, sin navegación (Recipes no conoce el modelo de Security)
        b.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(r => r.AuthorId);

        // Foto: si la imagen se borra, la receta queda sin foto
        b.HasOne<Image>()
            .WithMany()
            .HasForeignKey(r => r.ImageId)
            .OnDelete(DeleteBehavior.SetNull);

        // Agregado: Recipe es dueño de sus ingredientes y pasos
        b.HasMany(r => r.Ingredients)
            .WithOne()
            .HasForeignKey(i => i.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasMany(r => r.Steps)
            .WithOne()
            .HasForeignKey(s => s.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

        // EF lee/escribe los campos privados _ingredients y _steps
        b.Navigation(r => r.Ingredients).UsePropertyAccessMode(PropertyAccessMode.Field);
        b.Navigation(r => r.Steps).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
