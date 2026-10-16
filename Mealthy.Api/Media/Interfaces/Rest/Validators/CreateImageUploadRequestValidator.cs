using FluentValidation;
using Mealthy.Api.Media.Interfaces.Rest.Resources;

namespace Mealthy.Api.Media.Interfaces.Rest.Validators;

/// <summary>Solo forma: el tipo permitido y el tamaño máximo los decide ImageService con MediaOptions.</summary>
public sealed class CreateImageUploadRequestValidator : AbstractValidator<CreateImageUploadRequest>
{
    public CreateImageUploadRequestValidator()
    {
        RuleFor(x => x.ContentType).NotEmpty().MaximumLength(50).WithName("Tipo de archivo");
        RuleFor(x => x.SizeBytes).GreaterThan(0).WithName("Tamaño");
    }
}
