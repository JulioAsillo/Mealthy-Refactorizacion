namespace Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;

public class AuthenticationFailedException(string message) : Exception(message);
