using Domain.Interfaces;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration, Microsoft.Extensions.Hosting.IHostEnvironment? environment = null)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        var isDevelopment = environment?.IsDevelopment() ?? false;
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(maxRetryCount: 3);
                npgsqlOptions.CommandTimeout(30);
            });
        });

        services.AddDefaultIdentity<Infrastructure.Data.ApplicationUser>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
        })
        .AddRoles<Microsoft.AspNetCore.Identity.IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>();

        services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Account/Login";
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpClient<ICarPricingService, CarPricingService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrencyService, FreeCurrencyService>();
        services.AddScoped<IGeolocationService, IpGeolocationService>();

        return services;
    }
}
