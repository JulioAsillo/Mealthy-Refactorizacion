using System.Diagnostics;

namespace Mealthy.Api.Media.Infraestructure;

/// <summary>Trazas propias del contexto. ServiceDefaults escucha "Mealthy.Api.*", así que salen en el dashboard de Aspire.</summary>
internal static class MediaTelemetry
{
    public static readonly ActivitySource Source = new("Mealthy.Api.Media");
}
