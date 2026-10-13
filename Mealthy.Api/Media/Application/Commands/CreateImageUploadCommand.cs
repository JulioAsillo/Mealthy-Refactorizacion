namespace Mealthy.Api.Media.Application.Commands;

public record CreateImageUploadCommand(string ContentType, long SizeBytes);
