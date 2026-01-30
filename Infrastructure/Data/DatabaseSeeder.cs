using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Collections.Generic;

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
                new Category { Id = 1, Name = "Cars", Description = "Used cars for sale" },
                new Category { Id = 2, Name = "Motorcycles", Description = "Used motorcycles for sale" },
                new Category { Id = 3, Name = "Parts", Description = "Car and motorcycle parts" }
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
        if (existingCount < 30)
        {
            var listingsList = new List<Listing>();
            for (int i = existingCount + 1; i <= 30; i++)
            {
                var categoryId = (i % 3) == 1 ? 1 : ((i % 3) == 2 ? 2 : 3);
                var title = categoryId == 1 ? $"Used Car Model {i}" : categoryId == 2 ? $"Motorcycle Model {i}" : $"Part #{i}";
                var make = categoryId == 3 ? "" : $"Make{i % 10}";
                var model = categoryId == 3 ? $"PartModel{i % 20}" : $"Model{i % 20}";
                var year = 2000 + (i % 25);
                var price = categoryId == 3 ? 10 + i * 2 : 1000 + i * 150;
                var imageIndex = ((i - 1) % 10) + 1;
                var imageUrl = $"/images/listings/listing{imageIndex}.jpg";

                listingsList.Add(new Listing
                {
                    UserId = testUser!.Id,
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
