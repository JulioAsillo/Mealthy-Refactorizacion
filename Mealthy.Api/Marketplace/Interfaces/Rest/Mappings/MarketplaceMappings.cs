using Mealthy.Api.Marketplace.Application.Commands;
using Mealthy.Api.Marketplace.Domain.Model;
using Mealthy.Api.Marketplace.Interfaces.Rest.Resources;

namespace Mealthy.Api.Marketplace.Interfaces.Rest.Mappings;

/// <summary>Mapeo manual Resource ↔ Command / Entity ↔ Resource (sin AutoMapper).</summary>
public static class MarketplaceMappings
{
    public static CreateStoreCommand ToCommand(this CreateStoreRequest r) =>
        new(r.Name, r.Description, r.Ruc, r.PhotoUrl);

    public static UpdateStoreCommand ToCommand(this UpdateStoreRequest r) =>
        new(r.Name, r.Description, r.PhotoUrl);

    public static ProductCommand ToCommand(this ProductRequest r) =>
        new(r.Name, r.Category, r.Price, r.Unit, r.Stock, r.PhotoUrl);

    public static StoreResponse ToResponse(this Store s) =>
        new(s.Id, s.Name, s.Description, s.Ruc, s.PhotoUrl, s.OwnerId, s.CreatedAt);

    public static ProductResponse ToResponse(this Product p) =>
        new(p.Id, p.StoreId, p.Name, p.Category, p.Price, p.Unit, p.Stock, p.PhotoUrl, p.CreatedAt, p.UpdatedAt);
}
