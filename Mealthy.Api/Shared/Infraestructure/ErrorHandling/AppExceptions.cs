namespace Mealthy.Api.Shared.Infraestructure.ErrorHandling;

public class NotFoundException(string message) : Exception(message);
public class ConflictException(string message) : Exception(message);
public class BusinessRuleException(string message) : Exception(message);