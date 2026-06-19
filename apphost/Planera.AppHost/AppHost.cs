using Projects;

var builder = DistributedApplication.CreateBuilder(args);

// Backend
var postgres = builder
    .AddPostgres("Postgres")
    .WithLifetime(ContainerLifetime.Persistent);
var postgresDatabase = postgres.AddDatabase("PlaneraDatabase");

var api = builder.AddProject<Planera_Api>("PlaneraApi")
    .WithHttpEndpoint(5056)
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase);

// Frontend
builder
    .AddViteApp("PlaneraWeb", "../../web")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(3000);

builder.Build().Run();
