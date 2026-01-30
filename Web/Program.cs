using Application;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"=== PORT Configuration ===");
Console.WriteLine($"PORT env variable: {Environment.GetEnvironmentVariable("PORT") ?? "NOT SET"}");
Console.WriteLine($"ASPNETCORE_URLS env: {Environment.GetEnvironmentVariable("ASPNETCORE_URLS") ?? "NOT SET"}");
Console.WriteLine($"Using port: {port}");
Console.WriteLine($"All environment variables:");
foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
{
    if (env.Key.ToString().Contains("PORT") || env.Key.ToString().Contains("URL") || env.Key.ToString().Contains("HOST"))
    {
        Console.WriteLine($"  {env.Key}: {env.Value}");
    }
}
Console.WriteLine($"========================");

if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("ASPNETCORE_URLS")))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllersWithViews()
    .AddViewLocalization(Microsoft.AspNetCore.Mvc.Razor.LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

builder.Services.AddInfrastructure(builder.Configuration, builder.Environment);

builder.Services.AddApplication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Attempting to connect to database...");
        var context = services.GetRequiredService<Infrastructure.Data.ApplicationDbContext>();

        await context.Database.MigrateAsync();
        await context.Database.CanConnectAsync();
        logger.LogInformation("Database connection successful!");

        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }

        logger.LogInformation("Starting database seeding...");
        await Infrastructure.Data.DatabaseSeeder.SeedAsync(context, services);
        logger.LogInformation("Database seeding completed successfully!");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while seeding the database. Application will continue but may not function properly.");
        logger.LogError("Connection String (masked): {ConnectionString}",
            builder.Configuration.GetConnectionString("DefaultConnection")?.Substring(0, Math.Min(50, builder.Configuration.GetConnectionString("DefaultConnection")?.Length ?? 0)));
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

var supportedCultures = new[] { "en-US", "mk-MK" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);
app.UseMiddleware<Web.Middleware.GeolocationLocalizationMiddleware>();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

if (args.Contains("--migrate-admin"))
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Infrastructure.Data.ApplicationUser>>();
        
        var email = "admin@carparts.com";
        var password = "Admin@123456";
        
        var user = await userManager.FindByEmailAsync(email);
        if (user != null)
        {
            await userManager.DeleteAsync(user);
        }
        
        var newUser = new Infrastructure.Data.ApplicationUser
        {
            UserName = "admin",
            Email = email,
            EmailConfirmed = true,
            FirstName = "Admin",
            LastName = "User"
        };
        
        var result = await userManager.CreateAsync(newUser, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(newUser, "Admin");
            Console.WriteLine($"✓ Admin user created successfully!");
            Console.WriteLine($"  Email: {email}");
            Console.WriteLine($"  Password: {password}");
        }
        else
        {
            Console.WriteLine($"✗ Failed to create admin user:");
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  - {error.Description}");
            }
        }
    }
    return;
}

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
