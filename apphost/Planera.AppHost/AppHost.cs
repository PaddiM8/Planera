using Aspire.Hosting.Yarp.Transforms;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Backend
var postgres = builder
    .AddPostgres("Postgres")
    .WithHostPort(5433)
    .WithLifetime(ContainerLifetime.Persistent);
var postgresDatabase = postgres.AddDatabase("PlaneraDatabase");

var api = builder.AddProject<Planera_Api>("PlaneraApi")
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase);

// Frontend
var frontend = builder
    .AddViteApp("PlaneraWeb", "../../web")
    .WithHttpEndpoint(port: 3000)
    .WithNpm();

// Frontend + Backend
var gateway = builder
    .AddYarp("gateway")
    .WithHttpEndpoint(port: 2001)
    .WithHttpsEndpoint(port: 2002)
    .WithConfiguration(yarp =>
    {
        yarp.AddRoute(frontend);
        yarp
            .AddRoute("/api/{**catch-all}", api)
            .WithTransformPathRemovePrefix("/api");
    });

api.WithEnvironment("PLANERA_FRONTEND_URL", gateway.GetEndpoint("https"));
frontend
    .WithReference(api)
    .WaitFor(api)
    .WithEnvironment("ORIGIN", gateway.GetEndpoint("https"))
    .WithEnvironment("VITE_PUBLIC_API_URL", $"{gateway.GetEndpoint("https")}/api");


builder.Build().Run();
