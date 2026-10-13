// ─────────────────────────────────────────────────────────────────────────────
// Mealthy AppHost (.NET Aspire): orquesta todo el entorno local con un solo F5.
// Reemplaza a compose.yaml. Dashboard (logs, trazas, métricas) en la URL que
// imprime la consola al arrancar.
// ─────────────────────────────────────────────────────────────────────────────

var builder = DistributedApplication.CreateBuilder(args);

// ---------- PostgreSQL ----------
// Contenedor persistente: no se recrea entre ejecuciones del AppHost (arranque rápido).
// La contraseña la genera Aspire y la guarda en los user-secrets del AppHost.
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume("mealthy-pgdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db = postgres.AddDatabase("mealthy");

// ---------- Azure Storage (Azurite en local) ----------
// Blob: originales privados + variantes WebP públicas. Queue: trabajo para el worker de imágenes.
// Mismo SDK y mismo código que contra una Storage Account real de Azure.
var storage = builder.AddAzureStorage("storage")
    .RunAsEmulator(azurite => azurite
        .WithDataVolume("mealthy-azurite")
        .WithLifetime(ContainerLifetime.Persistent));

var blobs = storage.AddBlobs("blobs");
var queues = storage.AddQueues("queues");

// ---------- Redis ----------
// L2 de HybridCache: compartido entre instancias de la API y sobrevive a reinicios del proceso.
var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent);

// ---------- API ----------
// connectionName "DefaultConnection": la API sigue leyendo GetConnectionString("DefaultConnection"),
// así no hace falta tocar su configuración. Aspire la inyecta como variable de entorno
// (ConnectionStrings__DefaultConnection), que tiene prioridad sobre los user-secrets de la API.
var api = builder.AddProject<Projects.Mealthy_Api>("api")
    .WithReference(db, connectionName: "DefaultConnection")
    .WithReference(blobs)
    .WithReference(queues)
    .WithReference(cache)
    .WaitFor(db)
    .WaitFor(blobs)
    .WaitFor(queues)
    .WaitFor(cache)
    .WithHttpHealthCheck("/health");

// ---------- Frontend (repo separado: mealthy-frontend) ----------
// Ruta relativa a la carpeta del AppHost. Se puede cambiar sin tocar código:
//   dotnet user-secrets set "Frontend:Path" "C:/ruta/a/mealthy-frontend" --project Mealthy.AppHost
var frontendPath = Path.GetFullPath(
    builder.Configuration["Frontend:Path"] ?? "../../mealthy-frontend",
    builder.AppHostDirectory);

if (Directory.Exists(frontendPath))
{
    var web = builder.AddViteApp("web", frontendPath)
        // Puerto público fijo (proxy de Aspire): mismo origen de siempre para CORS y marcadores.
        .WithEndpoint("http", e => e.Port = 5173)
        // Sustituye al .env local: Vite solo expone variables con prefijo VITE_.
        .WithEnvironment("VITE_API_BASE_URL", ReferenceExpression.Create($"{api.GetEndpoint("http")}/api/v1"))
        .WithEnvironment("VITE_APP_NAME", "Mealthy")
        .WaitFor(api);

    // CORS de la API = origen real del frontend (una sola fuente de verdad).
    api.WithEnvironment("Cors__AllowedOrigins__0", web.GetEndpoint("http"));
}

builder.Build().Run();
