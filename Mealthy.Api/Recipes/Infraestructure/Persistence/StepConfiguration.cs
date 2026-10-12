using Mealthy.Api.Recipes.Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mealthy.Api.Recipes.Infraestructure.Persistence;

public class StepConfiguration : IEntityTypeConfiguration<Step>
{
    public void Configure(EntityTypeBuilder<Step> b)
    {
        b.ToTable("steps");
        b.HasKey(s => s.Id);

        b.Property(s => s.Description).HasMaxLength(1000).IsRequired();
        b.Property(s => s.Order).IsRequired();

        // Índice NO único: Resequence() reordena varios pasos en un mismo SaveChanges
        b.HasIndex(s => new { s.RecipeId, s.Order });
    }
}
