using Mealthy.Api.Marketplace.Application.Services;
using Mealthy.Api.Marketplace.Domain.Repositories;
using Mealthy.Api.Marketplace.Infraestructure.Persistence;
using Mealthy.Api.Media.Application.Contracts;

namespace Mealthy.Api.Marketplace;

public static class MarketplaceModule
{
    public static IServiceCollection AddMarketplaceModule(this IServiceCollection services)
    {
        services.AddScoped<IStoreRepository, StoreRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<StoreService>();
        services.AddScoped<ProductService>();
        services.AddScoped<IImageReferenceSource, MarketplaceImageReferences>();   // evita que Media borre fotos en uso
        return services;
    }
}
