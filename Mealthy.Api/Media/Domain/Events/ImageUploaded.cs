using Mealthy.Api.Shared.Domain.Model;

namespace Mealthy.Api.Media.Domain.Events;

/// <summary>El archivo llegó al storage y la imagen pasó a Processing: hay que generar sus variantes.</summary>
public sealed record ImageUploaded(int ImageId) : IDomainEvent;
