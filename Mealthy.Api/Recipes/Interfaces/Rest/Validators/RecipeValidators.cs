using FluentValidation;
using Mealthy.Api.Recipes.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Interfaces.Rest.Validation;

namespace Mealthy.Api.Recipes.Interfaces.Rest.Validators;

public sealed class IngredientRequestValidator : AbstractValidator<IngredientRequest>
{
    public IngredientRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100).WithName("Nombre del ingrediente");
        RuleFor(x => x.Quantity).InclusiveBetween(0.01m, 99_999_999.99m).WithName("Cantidad");
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(20).WithName("Unidad");
    }
}

public sealed class StepRequestValidator : AbstractValidator<StepRequest>
{
    public StepRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000).WithName("Descripción del paso");
    }
}

/// <summary>Reglas comunes a crear y editar (título, descripción, tiempo, porciones, foto).</summary>
internal static class RecipeRules
{
    public static void Apply<T>(AbstractValidator<T> v,
        System.Linq.Expressions.Expression<Func<T, string>> title,
        System.Linq.Expressions.Expression<Func<T, string>> description,
        System.Linq.Expressions.Expression<Func<T, int>> minutes,
        System.Linq.Expressions.Expression<Func<T, int>> servings,
        System.Linq.Expressions.Expression<Func<T, int?>> imageId)
    {
        v.RuleFor(title).NotEmpty().Length(3, 120).WithName("Título");
        v.RuleFor(description).NotEmpty().MaximumLength(2000).WithName("Descripción");
        v.RuleFor(minutes).InclusiveBetween(1, 1440).WithName("Tiempo de preparación");
        v.RuleFor(servings).InclusiveBetween(1, 100).WithName("Porciones");
        v.RuleFor(imageId).OptionalImageId();
    }
}

public sealed class CreateRecipeRequestValidator : AbstractValidator<CreateRecipeRequest>
{
    public CreateRecipeRequestValidator()
    {
        RecipeRules.Apply(this, x => x.Title, x => x.Description, x => x.PreparationMinutes, x => x.Servings, x => x.ImageId);

        RuleFor(x => x.Ingredients).Must(list => list is null || list.Count <= 50)
            .WithMessage("Una receta puede tener como máximo 50 ingredientes.");
        RuleForEach(x => x.Ingredients).SetValidator(new IngredientRequestValidator());

        RuleFor(x => x.Steps).Must(list => list is null || list.Count <= 50)
            .WithMessage("Una receta puede tener como máximo 50 pasos.");
        RuleForEach(x => x.Steps).SetValidator(new StepRequestValidator());
    }
}

public sealed class UpdateRecipeRequestValidator : AbstractValidator<UpdateRecipeRequest>
{
    public UpdateRecipeRequestValidator()
        => RecipeRules.Apply(this, x => x.Title, x => x.Description, x => x.PreparationMinutes, x => x.Servings, x => x.ImageId);
}
