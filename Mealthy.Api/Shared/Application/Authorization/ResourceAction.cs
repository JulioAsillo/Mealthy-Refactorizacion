namespace Mealthy.Api.Shared.Application.Authorization;

public enum ResourceAction
{
    /// <summary>Crear hijos, editar. Solo el dueño.</summary>
    Manage,

    /// <summary>Eliminar. El dueño, o un Admin (moderación de contenido).</summary>
    Delete
}
