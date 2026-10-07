using System.Text.Json.Serialization;
using Mealthy.Api.Security;
using Mealthy.Api.Shared.Domain.Repositories;
using Mealthy.Api.Shared.Infraestructure.ErrorHandling;
using Mealthy.Api.Shared.Infraestructure.OpenApi;
using Mealthy.Api.Shared.Infraestructure.Persistence;
using Mealthy.Api.Shared.Infraestructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddScoped<IUnitOfWorks, UnitOfWork>();

// ---------- Bounded contexts ----------
builder.Services.AddSecurityModule(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();   // 401/403 del pipeline de auth también salen como ProblemDetails

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o => o.AddPreferredSecuritySchemes(BearerSecuritySchemeTransformer.SchemeName));
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program;   // para WebApplicationFactory en Fase 5