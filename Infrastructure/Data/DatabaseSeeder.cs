using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        await context.Database.EnsureCreatedAsync();

        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new IdentityRole("Admin"));
        }
        var adminEmail = "admin@carparts.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            var newAdminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User"
            };

            var result = await userManager.CreateAsync(newAdminUser, "Admin@123456");
            if (result.Succeeded)
            {
                logger.LogInformation("Admin user created successfully");
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.LogError($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            logger.LogInformation("Admin user already exists");
        }

        if (!await context.Categories.AnyAsync())
        {
            var categories = new[]
            {
                new Category { Id = 1, Name = "Cars",        Description = "Used cars for sale" },
                new Category { Id = 2, Name = "Motorcycles", Description = "Used motorcycles for sale" },
                new Category { Id = 3, Name = "Parts",       Description = "Car and motorcycle parts" }
            };

            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
        }

        var testUserEmail = "test@carparts.com";
        var testUser = await userManager.FindByEmailAsync(testUserEmail);
        if (testUser == null)
        {
            var newTestUser = new ApplicationUser
            {
                UserName = "testuser",
                Email = testUserEmail,
                EmailConfirmed = true,
                FirstName = "Test",
                LastName = "User",
                Country = "North Macedonia"
            };

            var testResult = await userManager.CreateAsync(newTestUser, "Test@123456");
            if (testResult.Succeeded)
            {
                logger.LogInformation("Test user created successfully");
                testUser = await userManager.FindByEmailAsync(testUserEmail);
            }
            else
            {
                var errors = string.Join(", ", testResult.Errors.Select(e => e.Description));
                logger.LogError($"Failed to create test user: {errors}");
            }
        }
        else
        {
            logger.LogInformation("Test user already exists");
        }

        var existingCount = await context.Listings.CountAsync();
        if (existingCount < 30 && testUser != null)
        {
            var listingsList = new List<Listing>();

            var imageUrls = new[]
            {
                "https://images.pexels.com/photos/210019/pexels-photo-210019.jpeg",
                "https://images.pexels.com/photos/1402787/pexels-photo-1402787.jpeg",
                "https://images.pexels.com/photos/112460/pexels-photo-112460.jpeg",
                "https://images.pexels.com/photos/2449452/pexels-photo-2449452.jpeg",
                "https://images.pexels.com/photos/799443/pexels-photo-799443.jpeg"
            };

            var carMakes  = new[] { "Toyota", "BMW", "Audi", "Ford", "Volkswagen" };
            var carModels = new[] { "Corolla", "3 Series", "A4", "Focus", "Golf" };

            var motoMakes  = new[] { "Honda", "Yamaha", "Kawasaki", "Suzuki", "Ducati" };
            var motoModels = new[] { "CBR600", "MT-07", "Ninja 650", "GSX-R750", "Monster 821" };

            var partNames = new[]
            {
                "Brake Pads", "Oil Filter", "Spark Plugs", "Air Filter", "Clutch Kit",
                "Battery", "Alternator", "Radiator", "Fuel Pump", "Headlights"
            };

            for (int i = existingCount + 1; i <= 30; i++)
            {
                var categoryId = (i % 3) == 1 ? 1 : ((i % 3) == 2 ? 2 : 3);
                var year = 2000 + (i % 25);
                var price = categoryId == 3 ? 10 + i * 2 : 1000 + i * 150;
                var imageUrl = imageUrls[Random.Shared.Next(imageUrls.Length)];

                string title;
                string make;
                string model;

                if (categoryId == 1) // Cars
                {
                    make  = carMakes[(i - 1) % carMakes.Length];
                    model = carModels[(i - 1) % carModels.Length];
                    title = $"{make} {model} {year}";
                }
                else if (categoryId == 2) // Motorcycles
                {
                    make  = motoMakes[(i - 1) % motoMakes.Length];
                    model = motoModels[(i - 1) % motoModels.Length];
                    title = $"{make} {model} {year}";
                }
                else // Parts
                {
                    make  = string.Empty;
                    model = partNames[(i - 1) % partNames.Length];
                    title = model;
                }

                listingsList.Add(new Listing
                {
                    UserId = testUser.Id,
                    CategoryId = categoryId,
                    Title = title,
                    Description = $"Auto-generated listing {i}",
                    Make = make,
                    Model = model,
                    Year = year,
                    Condition = "Good",
                    Price = price,
                    Currency = "USD",
                    ListingType = "Sale",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrl = imageUrl
                });
            }

            await context.Listings.AddRangeAsync(listingsList);
            await context.SaveChangesAsync();
        }
    }
}
