using Mealthy.Api.Media.Application.Contracts;
using Mealthy.Api.Media.Domain.Model;

namespace Mealthy.Api.Media.Application.Results;

public record ImageUpload(Image Image, UploadTarget Target, DateTimeOffset ExpiresAt);
