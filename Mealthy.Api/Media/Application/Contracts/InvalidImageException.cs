namespace Mealthy.Api.Media.Application.Contracts;

/// <summary>El archivo no es procesable (formato, tamaño o resolución). No se reintenta.</summary>
public class InvalidImageException(string message) : Exception(message);
