using Mealthy.Api.Shared.Infraestructure.ErrorHandling.Exceptions;
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
            AuthenticationFailedException => (StatusCodes.Status401Unauthorized, "No autenticado"),
            ForbiddenException     => (StatusCodes.Status403Forbidden, "Acceso denegado"),
            // Carrera entre el ExistsBy... y el INSERT: el índice único de Postgres (23505) responde
            Microsoft.EntityFrameworkCore.DbUpdateException { InnerException: Npgsql.PostgresException { SqlState: "23505" } }
                                   => (StatusCodes.Status409Conflict, "Conflicto"),
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
                Detail = status is 500 || ex is Microsoft.EntityFrameworkCore.DbUpdateException ? null : ex.Message 
            }
        });
    }
}