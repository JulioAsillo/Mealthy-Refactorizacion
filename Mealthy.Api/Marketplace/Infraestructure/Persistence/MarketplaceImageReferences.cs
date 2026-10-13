using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Shared.Infraestructure.Persistence;

namespace Mealthy.Api.Marketplace.Infraestructure.Persistence;

/// <summary>Imágenes en uso por Marketplace: fotos de tiendas y de productos.</summary>
public class MarketplaceImageReferences(AppDbContext context) : IImageReferenceSource
{
    public IQueryable<int> ReferencedImageIds()
        => context.Set<Store>().Where(s => s.ImageId != null).Select(s => s.ImageId!.Value)
            .Union(context.Set<Product>().Where(p => p.ImageId != null).Select(p => p.ImageId!.Value));
}
