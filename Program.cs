using Microsoft.EntityFrameworkCore;
using WeatherSeaWebApplication.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WeatherSeaWebApplication.Controllers;

namespace WeatherSeaWebApplication
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Добавьте строку подключения к базе данных в appsettings.json
            var connectionString = builder.Configuration.GetConnectionString("FieldListConnection");

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddDbContext<AuthorizationContext>(options =>
                options.UseNpgsql(connectionString));
            builder.Services.AddDbContext<ModulesContext>(options =>
                options.UseNpgsql(connectionString));

            // Получите секретный ключ из appsettings.json
            var secretKey = builder.Configuration["Jwt:Key"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
        .AddJwtBearer(options =>
        {
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




            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}");

            app.MapControllerRoute(
                name: "account",
                pattern: "{controller=Account}/{action=Register}");

            app.MapControllerRoute(
                name: "modules",
                pattern: "{controller=Modules}/{action=FieldList}");

            app.Run();
        }
    }
}
