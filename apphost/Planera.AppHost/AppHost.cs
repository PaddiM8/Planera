#pragma warning disable ASPIREPIPELINES003
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var registry = builder.AddContainerRegistry("Registry", "registry.strct.net");
var k8s = builder
    .AddKubernetesEnvironment("k8s")
    .WithContainerRegistry(registry);

// Backend
var postgres = builder
    .AddPostgres("Postgres")
    .WithImagePushOptions(x => x.Options.RemoteImageName = "bakk/planera-db")
    .WithLifetime(ContainerLifetime.Persistent);
var postgresDatabase = postgres.AddDatabase("PlaneraDatabase");

var api = builder.AddProject<Planera_Api>("PlaneraApi")
    .WithImagePushOptions(x => x.Options.RemoteImageName = "bakk/planera-api")
    .WithHttpEndpoint(5056)
    .WithReference(postgresDatabase)
    .WaitFor(postgresDatabase)
    .WithEnvironment("PLANERA_FRONTEND_URL", "http://localhost:2000");

// Frontend
var web = builder
    .AddViteApp("PlaneraWeb", "../../web")
    .WithImagePushOptions(x => x.Options.RemoteImageName = "bakk/planera-web")
    .WithNpm()
    .WithReference(api)
    .WaitFor(api)
    .WithHttpEndpoint(3000)
    .WithEnvironment("ORIGIN", "http://localhost:2000")
    .WithEnvironment("VITE_PUBLIC_API_URL", "https://localhost:2000/api");

// Caddy Reverse Proxy
builder.AddContainer("caddy", "caddy", "2.7-alpine")
    .WithImagePushOptions(x => x.Options.RemoteImageName = "bakk/planera-proxy")
    .WithHttpEndpoint(targetPort: 80, port: 2000, name: "http")
    .WithHttpEndpoint(targetPort: 443, port: 2443, name: "https")
    .WithBindMount("../../Caddyfile", "/etc/caddy/Caddyfile")
    .WithReference(api)
    .WithReference(web)
    .WaitFor(api)
    .WaitFor(web);

builder.Build().Run();
