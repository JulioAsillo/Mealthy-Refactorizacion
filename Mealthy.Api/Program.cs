using System.Text.Json.Serialization;
using Mealthy.Api.Marketplace;
using Mealthy.Api.Media;
using Mealthy.Api.Pantry;
using Mealthy.Api.Recipes;
using Mealthy.Api.Security;
using Mealthy.Api.Security.Infraestructure.Persistence;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling;
using Mealthy.Api.Shared.Infraestructure.OpenApi;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Mealthy.Api.Shared.Infraestructure.Web;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ---------- Aspire: OpenTelemetry, health "self", resiliencia HTTP ----------
builder.AddServiceDefaults();

// ---------- Shared ----------

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddRouting(o => o.LowercaseUrls = true);
builder.Services.AddOpenApi(o => o.AddDocumentTransformer<BearerSecuritySchemeTransformer>());
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
        .UseSnakeCaseNamingConvention();
    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging().EnableDetailedErrors();
});
// Reintentos ante fallos transitorios, health check "AppDbContext" y trazas de EF en el dashboard
builder.EnrichNpgsqlDbContext<AppDbContext>();
builder.Services.AddScoped<IUnitOfWorks, UnitOfWork>();

builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddApiRateLimiting();
builder.AddApiPerformance();   // Redis + HybridCache, output cache del catálogo, compresión

// ---------- Bounded contexts ----------
builder.AddMediaModule();   // Blob/Queue Storage, workers y el contrato IImageAttachmentValidator
builder.Services.AddSecurityModule(builder.Configuration);
builder.Services.AddRecipesModule();
builder.Services.AddPantryModule();
builder.Services.AddMarketplaceModule();

var app = builder.Build();

app.UseResponseCompression();   // primero: comprime todo lo que salga, incluidas respuestas del caché
app.UseExceptionHandler();
app.UseStatusCodePages();   // 401/403 del pipeline de auth también salen como ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName));
}
else
{
    // En dev no: un redirect 307 a https rompe el preflight CORS del frontend en http
    app.UseHttpsRedirection();
}

// Orden importa: CORS antes de auth (el preflight OPTIONS no lleva token) y el rate limiter
// después de autenticar, para poder limitar por usuario (política "uploads").
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();
app.UseOutputCache();   // después de CORS y de auth; solo actúa en endpoints con [OutputCache]

app.MapControllers();
app.MapApiHealthChecks();

await app.MigrateDatabaseAsync();   // solo Development
await app.SeedAdminAsync();

app.Run();

public partial class Program;
