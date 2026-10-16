using FluentValidation;
using Mealthy.Api.Marketplace.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Interfaces.Rest.Validation;

namespace Mealthy.Api.Marketplace.Interfaces.Rest.Validators;

public sealed class CreateStoreRequestValidator : AbstractValidator<CreateStoreRequest>
{
    /// <summary>RUC peruano: 11 dígitos, empieza en 10 (persona natural), 15, 17 o 20 (persona jurídica).</summary>
    public const string RucPattern = @"^(10|15|17|20)\d{9}$";

    public CreateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100).WithName("Nombre de la tienda");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000).WithName("Descripción");
        RuleFor(x => x.Ruc).NotEmpty().Matches(RucPattern)
            .WithMessage("RUC inválido: deben ser 11 dígitos y empezar en 10, 15, 17 o 20.");
        RuleFor(x => x.ImageId).OptionalImageId();
    }
}

public sealed class UpdateStoreRequestValidator : AbstractValidator<UpdateStoreRequest>
{
    public UpdateStoreRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 100).WithName("Nombre de la tienda");
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000).WithName("Descripción");
        RuleFor(x => x.ImageId).OptionalImageId();
    }
}

public sealed class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().Length(2, 120).WithName("Nombre");
        RuleFor(x => x.Category).NotEmpty().Length(2, 50).WithName("Categoría");
        RuleFor(x => x.Price).InclusiveBetween(0.01m, 99_999_999.99m).WithName("Precio");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20).WithName("Unidad");
        RuleFor(x => x.Stock).GreaterThanOrEqualTo(0).WithName("Stock");
        RuleFor(x => x.ImageId).OptionalImageId();
    }
}
