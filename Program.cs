using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WeatherSeaWebApplication.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Добавьте строку подключения к базе данных в appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DatabaseConnection");

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<JwtGenerator>();
builder.Services.AddScoped<IdentificationResponseHandler>();

NpgsqlConnection.GlobalTypeMapper.EnableUnmappedTypes();

NpgsqlConnection.GlobalTypeMapper.MapEnum<UserType>("usertype");
NpgsqlConnection.GlobalTypeMapper.MapEnum<FieldType>("fieldtype");
NpgsqlConnection.GlobalTypeMapper.MapEnum<EntityClass>("entityclass");
NpgsqlConnection.GlobalTypeMapper.MapEnum<EntityOrigin>("entityorigin");
NpgsqlConnection.GlobalTypeMapper.MapEnum<ReportType>("reporttype");
NpgsqlConnection.GlobalTypeMapper.MapEnum<AccessType>("accesstype");

builder.Services.AddDbContext<AuthorizationContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddDbContext<ModulesContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
    options.Cookie.SameSite = SameSiteMode.Strict; 
});

var secretKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("Секретный ключ не найден в конфигурации.");
}
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = key
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{Action=Index}");

app.Run();
