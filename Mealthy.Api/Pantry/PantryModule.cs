using Mealthy.Api.Pantry.Application.Services;
using Mealthy.Api.Pantry.Domain.Repositories;
using Mealthy.Api.Pantry.Infraestructure.Persistence;

namespace Mealthy.Api.Pantry;

public static class PantryModule
{
    public static IServiceCollection AddPantryModule(this IServiceCollection services)
    {
        services.AddScoped<ISupplyRepository, SupplyRepository>();
        services.AddScoped<PantryService>();
        return services;
    }
}
