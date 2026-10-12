using Mealthy.Api.Recipes.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Recipes.Infraestructure.Persistence;

public class IngredientConfiguration : IEntityTypeConfiguration<Ingredient>
{
    public void Configure(EntityTypeBuilder<Ingredient> b)
    {
        b.ToTable("ingredients");
        b.HasKey(i => i.Id);

        b.Property(i => i.Name).HasMaxLength(100).IsRequired();
        b.Property(i => i.Quantity).HasPrecision(10, 2).IsRequired();
        b.Property(i => i.Unit).HasMaxLength(20).IsRequired();
    }
}
