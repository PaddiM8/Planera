using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Planera.Api;
using Planera.Api.Data;
using Planera.Api.Data.Files;
using Planera.Api.Hubs;
using Planera.Api.Models;
using Planera.Api.Services;
using Planera.Api.Utility;

Directory.CreateDirectory("./store");
Directory.CreateDirectory("./wwwroot");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "PLANERA_");

builder.AddServiceDefaults();

// Read/generate the JWT key
if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]))
{
    const string jwtKeyPath = "./store/jwt.key";
    if (File.Exists(jwtKeyPath))
    {
        var jwtKey = File.ReadAllText(jwtKeyPath);
        builder.Configuration["Jwt:Key"] = jwtKey;
    }
    else
    {
        var jwtKey = Generation.GenerateJwtKey();
        builder.Configuration["Jwt:Key"] = jwtKey;
        File.WriteAllText(jwtKeyPath, jwtKey);
    }
}

var serializerSettings = new JsonSerializerSettings
{
    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
    ContractResolver = new DefaultContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = true,
        },
    },
};
builder.Services
    .AddControllers()
    .AddNewtonsoftJson(o =>
    {
        o.SerializerSettings.ReferenceLoopHandling = serializerSettings.ReferenceLoopHandling;
        o.SerializerSettings.ContractResolver = serializerSettings.ContractResolver;
    });

builder.Services.AddOpenApiDocument();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddAuthorizationBuilder()
    .SetDefaultPolicy(new AuthorizationPolicyBuilder().AddAuthenticationSchemes("DynamicScheme").RequireAuthenticatedUser().Build());

builder.Services
    .AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(o =>
{
    o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    o.Lockout.MaxFailedAccessAttempts = 5;
    o.Lockout.AllowedForNewUsers = true;
    o.Password.RequireDigit = false;
    o.Password.RequireLowercase = false;
    o.Password.RequireNonAlphanumeric = false;
    o.Password.RequireUppercase = false;
    o.Password.RequiredLength = 8;
    o.Password.RequiredUniqueChars = 1;

});

var connectionString = builder.Configuration.GetConnectionString("PlaneraDatabase");
if (string.IsNullOrEmpty(connectionString))
{
    var host = builder.Configuration["Postgres:Host"];
    var user = builder.Configuration["Postgres:User"];
    var pass = builder.Configuration["Postgres:Password"];
    var db = builder.Configuration["Postgres:Db"];

    if (host != null)
    {
        var hostParts = host.Split(':');
        var server = hostParts[0];
        var port = hostParts.Length > 1 ? hostParts[1] : "5432";
        builder.Configuration["ConnectionStrings:PlaneraDatabase"] = $"Host={server};Port={port};Database={db};Username={user};Password={pass};";
    }
}

builder.AddNpgsqlDbContext<DataContext>("PlaneraDatabase");

var oidcConfig = builder.Configuration.GetSection("Oidc");
if (oidcConfig.Exists() && !string.IsNullOrWhiteSpace(oidcConfig["ProviderId"]))
    builder.Services.Configure<OidcOptions>(oidcConfig);

var authenticationBuilder = builder
    .Services
    .AddAuthentication("DynamicScheme")
    .AddPolicyScheme("DynamicScheme", "JWT or PAT", o =>
    {
        o.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader?.StartsWith("Pat ", StringComparison.OrdinalIgnoreCase) is true)
                return "PersonalAccessToken";

            return JwtBearerDefaults.AuthenticationScheme;
        };
    })
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
            ),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
        };

        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("token", out var jwtCookie))
                    context.Token = jwtCookie;

                return Task.CompletedTask;
            }
        };
    })
    .AddScheme<AuthenticationSchemeOptions, PersonalAccessTokenHandler>(
        "PersonalAccessToken",
        _ => {}
    );

if (oidcConfig.Exists() && !string.IsNullOrWhiteSpace(oidcConfig["ProviderId"]))
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        options.ForwardedHeaders = ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto;
        options.KnownIPNetworks.Clear();
        options.KnownProxies.Clear();
    });
    
    var callbackPath = string.IsNullOrEmpty(oidcConfig["CallbackPath"])
        ? "/api/signin-oidc"
        : oidcConfig["CallbackPath"]!;
    authenticationBuilder
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.Authority = oidcConfig["ProviderUrl"];
            options.ClientId = oidcConfig["ClientId"];
            options.ClientSecret = oidcConfig["ClientSecret"];

            foreach (var scope in oidcConfig["Scopes"]?.Split(' ') ?? [])
                options.Scope.Add(scope);

            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.ResponseType = OpenIdConnectResponseType.Code;

            options.SaveTokens = true;
            options.GetClaimsFromUserInfoEndpoint = true;

            options.MapInboundClaims = false;
            options.TokenValidationParameters.NameClaimType = JwtRegisteredClaimNames.Name;
            options.TokenValidationParameters.RoleClaimType = "roles";
            
            options.CorrelationCookie.Path = callbackPath;
            options.NonceCookie.Path = callbackPath;
            
            options.Events = new OpenIdConnectEvents
            {
                OnTokenValidated = context => OpenIdEventHandler.OnTokenValidatedAsync(
                    context,
                    builder.Configuration.GetValue<bool>("DisableRegistration")
                ),
                OnTicketReceived = OpenIdEventHandler.OnTicketReceivedAsync,
                OnRedirectToIdentityProvider = context => OpenIdEventHandler.OnRedirectToIdentityProvider(context, callbackPath),
            };
        });
}

builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol(options =>
    {
        options.PayloadSerializerSettings = serializerSettings;
    });

builder.Services.AddTransient<PlaneraAuthenticationService>();
builder.Services.AddTransient<PersonalAccessTokenService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<ProjectService>();
builder.Services.AddTransient<TicketService>();
builder.Services.AddTransient<IFileStorage, FileStorage>();
builder.Services.AddTransient<ImagePreparer>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<NoteService>();
builder.Services.AddAutoMapper(_ => { }, typeof(MappingProfile));

builder.Services.AddCors(o =>
    o.AddPolicy("DevelopmentCorsPolicy", cors =>
    {
        cors.AllowAnyMethod()
            .AllowAnyHeader()
            .SetIsOriginAllowed(_ => true)
            .AllowCredentials();
    })
);

var app = builder.Build();
app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
    app.UseCors("DevelopmentCorsPolicy");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app
    .MapControllers()
    .RequireAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");

app.MapHub<ProjectHub>("hubs/project");
app.MapHub<UserHub>("hubs/user");

if (oidcConfig.Exists())
{
    app.UseForwardedHeaders();
}

app.Services.GetService<IFileStorage>()?.CreateDirectory("avatars");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<DataContext>();
    if (context.Database.GetPendingMigrations().Any())
        context.Database.Migrate();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Failed to perform migrations: {ex}");
}

app.Run();
