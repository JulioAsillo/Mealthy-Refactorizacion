using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Mealthy.Api.Shared.Interfaces.Rest.Validation;

/// <summary>
/// Valida con FluentValidation cada argumento de la acción que tenga un IValidator&lt;T&gt; registrado.
/// Responde 400 con el mismo formato que la validación automática de [ApiController]
/// (ValidationProblemDetails con "errors" por campo), así el frontend no nota la diferencia.
///
/// Se usa un filtro propio en lugar de FluentValidation.AspNetCore porque su auto-validación está
/// discontinuada por el propio autor (no soporta validación asíncrona).
/// </summary>
public sealed class FluentValidationFilter(ProblemDetailsFactory problems) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null) continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator) continue;

            var result = await validator.ValidateAsync(
                new ValidationContext<object>(argument), context.HttpContext.RequestAborted);

            foreach (var error in result.Errors)
                context.ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        if (context.ModelState.IsValid)
        {
            await next();
            return;
        }

        var problem = problems.CreateValidationProblemDetails(
            context.HttpContext, context.ModelState, StatusCodes.Status400BadRequest,
            title: "Hay datos inválidos en el formulario");
        context.Result = new BadRequestObjectResult(problem);
    }
}
