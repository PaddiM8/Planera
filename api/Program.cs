using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Planera.Data;
using Planera.Hubs;
using Planera.Services;
using Planera.Utility;

Directory.CreateDirectory("./store");
Directory.CreateDirectory("./wwwroot");

var builder = WebApplication.CreateBuilder(args);

// Generate the JWT key if it isn't set
if (string.IsNullOrEmpty(builder.Configuration["Jwt:Key"]))
{
    builder.Configuration["Jwt:Key"] = Generation.GenerateJwtKey();
    var configJson = JsonConvert.DeserializeObject<JObject>(File.ReadAllText("appsettings.json"));
    configJson!["Jwt"]!["Key"] = builder.Configuration["Jwt:Key"];
    File.WriteAllText(
        "appsettings.json",
        JsonConvert.SerializeObject(configJson, Formatting.Indented)
    );
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
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("IdentityContextConnection"))
);
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

app.MapHub<ProjectHub>("/hubs/project");
app.MapHub<UserHub>("/hubs/user");

app.Run();
