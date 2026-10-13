using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Recipes.Application.Services;
using Mealthy.Api.Recipes.Domain.Repositories;
using Mealthy.Api.Recipes.Infraestructure.Persistence;

namespace Mealthy.Api.Recipes;

public static class RecipesModule
{
    public static IServiceCollection AddRecipesModule(this IServiceCollection services)
    {
        services.AddScoped<IRecipeRepository, RecipeRepository>();
        services.AddScoped<RecipeService>();
        services.AddScoped<IImageReferenceSource, RecipeImageReferences>();   // evita que Media borre fotos en uso
        return services;
    }
}
