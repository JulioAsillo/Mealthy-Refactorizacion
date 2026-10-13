using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Recipes.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.Persistence;

namespace Mealthy.Api.Recipes.Infraestructure.Persistence;

/// <summary>Imágenes en uso por Recipes: la foto de cada receta.</summary>
public class RecipeImageReferences(AppDbContext context) : IImageReferenceSource
{
    public IQueryable<int> ReferencedImageIds()
        => context.Set<Recipe>().Where(r => r.ImageId != null).Select(r => r.ImageId!.Value);
}
