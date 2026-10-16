using FluentValidation;
using Mealthy.Api.Security.Domain.Model;
using Mealthy.Api.Security.Interfaces.Rest.Resources;
using Mealthy.Api.Shared.Interfaces.Rest.Validation;

namespace Mealthy.Api.Security.Interfaces.Rest.Validators;

public sealed class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.Username).NotEmpty().Length(3, 30).Matches("^[a-zA-Z0-9_.]+$")
            .WithName("Usuario")
            .WithMessage("'{PropertyName}' solo admite letras, números, punto y guion bajo (3 a 30).");
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254).WithName("Email");
        RuleFor(x => x.Password).NotEmpty().Length(8, 72).WithName("Contraseña");
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80).WithName("Nombres");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80).WithName("Apellidos");
        RuleFor(x => x.Phone).PhoneNumber().When(x => !string.IsNullOrWhiteSpace(x.Phone)).WithName("Teléfono");
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            .When(x => x.BirthDate is not null)
            .WithMessage("La fecha de nacimiento no puede estar en el futuro.");
        RuleFor(x => x.Role).IsInEnum().NotEqual(Role.Admin)
            .WithMessage("No es posible registrarse con el rol Admin.");
    }
}

public sealed class SignInRequestValidator : AbstractValidator<SignInRequest>
{
    public SignInRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().WithName("Email");
        RuleFor(x => x.Password).NotEmpty().WithName("Contraseña");
    }
}

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator(TimeProvider clock)
    {
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(80).WithName("Nombres");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(80).WithName("Apellidos");
        RuleFor(x => x.Phone).PhoneNumber().When(x => !string.IsNullOrWhiteSpace(x.Phone)).WithName("Teléfono");
        RuleFor(x => x.BirthDate)
            .LessThanOrEqualTo(_ => DateOnly.FromDateTime(clock.GetUtcNow().UtcDateTime))
            .When(x => x.BirthDate is not null)
            .WithMessage("La fecha de nacimiento no puede estar en el futuro.");
    }
}
