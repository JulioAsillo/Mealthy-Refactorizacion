namespace Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

/// <summary>La versión enviada en If-Match ya no es la actual (otro usuario guardó antes) → 412.</summary>
public class PreconditionFailedException(string message) : Exception(message);
