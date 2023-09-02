using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Planera.Data;
using Planera.Data.Files;
using Planera.Hubs;
using Planera.Services;
using Planera.Utility;

Directory.CreateDirectory("./store");
Directory.CreateDirectory("./wwwroot");

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables(prefix: "PLANERA_");

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
builder.Services.AddControllers()
    .AddNewtonsoftJson(
    options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = serializerSettings.ReferenceLoopHandling;
        options.SerializerSettings.ContractResolver = serializerSettings.ContractResolver;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAuthorization();
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

});
builder.Services.AddDbContext<DataContext>();
builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? string.Empty)
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            if (context.Request.Cookies.TryGetValue("token", out var jwtCookie))
                context.Token = jwtCookie;

            return Task.CompletedTask;
        }
    };
});
builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol(options =>
    {
        options.PayloadSerializerSettings = serializerSettings;
    });

builder.Services.AddTransient<AuthenticationService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<ProjectService>();
builder.Services.AddTransient<TicketService>();
builder.Services.AddTransient<IFileStorage, FileStorage>();
builder.Services.AddTransient<ImagePreparer>();
builder.Services.AddTransient<EmailService>();
builder.Services.AddTransient<NoteService>();
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddCors(options =>
    options.AddPolicy("DevelopmentCorsPolicy", cors =>
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers()
    .RequireAuthorization();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action}/{id?}");

app.MapHub<ProjectHub>("hubs/project");
app.MapHub<UserHub>("hubs/user");

app.Services.GetService<IFileStorage>()?.CreateDirectory("avatars");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

var context = services.GetRequiredService<DataContext>();
if (false && context.Database.GetPendingMigrations().Any())
    context.Database.Migrate();

app.Run();
