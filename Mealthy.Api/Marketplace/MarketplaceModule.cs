using Mealthy.Api.Marketplace.Application.Services;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Marketplace.Infraestructure.Persistence;

namespace Mealthy.Api.Marketplace;

public static class MarketplaceModule
{
    public static IServiceCollection AddMarketplaceModule(this IServiceCollection services)
    {
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<StoreService>();
        services.AddScoped<ProductService>();
        return services;
    }
}
