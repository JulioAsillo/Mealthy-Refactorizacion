using System.Text.Json.Serialization;
using Mealthy.Api.Marketplace;
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

// ---------- Bounded contexts ----------
builder.Services.AddSecurityModule(builder.Configuration);
builder.Services.AddRecipesModule();
builder.Services.AddPantryModule();
builder.Services.AddMarketplaceModule();

var app = builder.Build();

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

// Orden importa: CORS antes de auth (el preflight OPTIONS no lleva token)
app.UseCors(CorsExtensions.FrontendPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapApiHealthChecks();

await app.MigrateDatabaseAsync();   // solo Development
await app.SeedAdminAsync();

app.Run();

public partial class Program;
