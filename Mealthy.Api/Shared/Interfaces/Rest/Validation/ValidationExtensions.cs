using System.Globalization;
using FluentValidation;

namespace Mealthy.Api.Shared.Interfaces.Rest.Validation;

public static class ValidationExtensions
{
    /// <summary>
    /// Registra todos los validadores del ensamblado (AbstractValidator&lt;T&gt;) y fija los mensajes
    /// por defecto en español. Cada módulo solo tiene que crear su clase XxxValidator.
    /// </summary>
    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        ValidatorOptions.Global.LanguageManager.Culture = new CultureInfo("es");
        services.AddValidatorsFromAssemblyContaining<Program>(includeInternalTypes: true);
        return services;
    }

    /// <summary>Teléfono flexible: dígitos, espacios, guiones, paréntesis y + inicial.</summary>
    public static IRuleBuilderOptions<T, string?> PhoneNumber<T>(this IRuleBuilder<T, string?> rule)
        => rule.MaximumLength(20).Matches(@"^\+?[0-9\s\-()]{6,20}$").WithMessage("'{PropertyName}' no es un teléfono válido.");

    /// <summary>Id de una imagen subida (opcional). La pertenencia y el estado los valida el contexto Media.</summary>
    public static IRuleBuilderOptions<T, int?> OptionalImageId<T>(this IRuleBuilder<T, int?> rule)
        => rule.GreaterThan(0).WithName("Imagen");
}
