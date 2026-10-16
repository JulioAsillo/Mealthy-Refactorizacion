using FluentValidation;
using Mealthy.Api.Pantry.Interfaces.Rest.Resources;

namespace Mealthy.Api.Pantry.Interfaces.Rest.Validators;

public sealed class SupplyRequestValidator : AbstractValidator<SupplyRequest>
{
    public SupplyRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithName("Nombre");
        RuleFor(x => x.Quantity).InclusiveBetween(0m, 99_999_999.99m).WithName("Cantidad");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20).WithName("Unidad");
    }
}
