namespace Mealthy.Api.Shared.Domain.Model;

/// <summary>
/// Entidad con control de concurrencia optimista. Version se mapea a la columna de sistema
/// xmin de PostgreSQL: cambia sola en cada UPDATE, sin columnas ni triggers extra.
/// El frontend la devuelve en la cabecera If-Match al editar.
/// </summary>
public interface IVersioned
{
    uint Version { get; }
}
