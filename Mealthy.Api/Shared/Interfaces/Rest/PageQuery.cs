namespace Mealthy.Api.Shared.Interfaces.Rest;

/// <summary>Parámetros de paginación del query string (?page=1&amp;pageSize=20), acotados.</summary>
public record PageQuery(int Page = 1, int PageSize = 20)
{
    public const int MaxPageSize = 100;
    public int SafePage => Math.Max(1, Page);
    public int SafePageSize => Math.Clamp(PageSize, 1, MaxPageSize);
}
