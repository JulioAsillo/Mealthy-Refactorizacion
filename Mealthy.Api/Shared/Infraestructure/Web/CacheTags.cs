namespace Mealthy.Api.Shared.Infraestructure.Web;

/// <summary>
/// Etiquetas del output cache. Una escritura invalida su etiqueta completa (listados + detalles),
/// así no hay que calcular qué URLs exactas quedaron obsoletas.
/// </summary>
public static class CacheTags
{
    public const string Recipes = "recipes";
    public const string Stores = "stores";
    public const string Products = "products";
}

/// <summary>Nombres de las políticas registradas en <see cref="PerformanceExtensions"/>.</summary>
public static class CachePolicies
{
    public const string Recipes = "catalog-recipes";
    public const string Stores = "catalog-stores";
    public const string Products = "catalog-products";
}
