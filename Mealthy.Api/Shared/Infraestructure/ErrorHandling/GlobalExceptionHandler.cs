using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Mealthy.Api.Shared.Infraestructure.ErrorHandling;


public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext http, Exception ex, CancellationToken ct)
    {
        var (status, title) = ex switch
        {
            NotFoundException      => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ConflictException      => (StatusCodes.Status409Conflict, "Conflicto"),
            BusinessRuleException  => (StatusCodes.Status400BadRequest, "Regla de negocio violada"),
            _                      => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        if (status == 500) logger.LogError(ex, "Excepción no controlada");

        http.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = http,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = status == 500 ? null : ex.Message 
            }
        });
    }
}