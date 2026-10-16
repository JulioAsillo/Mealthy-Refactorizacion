using Mealthy.Api.Shared.Domain.Model;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

namespace Mealthy.Api.Shared.Application;

public static class Concurrency
{
    /// <summary>
    /// Compara la versión que el cliente leyó (If-Match) con la actual. Null = el cliente no envió
    /// If-Match: no se comprueba aquí, pero EF Core sigue protegiendo la ventana entre leer y guardar (→ 409).
    /// </summary>
    public static void EnsureVersion(IVersioned entity, uint? expectedVersion)
    {
        if (expectedVersion is { } expected && expected != entity.Version)
            throw new PreconditionFailedException(
                "Alguien modificó este recurso después de que lo abriste. Recarga para ver los cambios.");
    }
}
