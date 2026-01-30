using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public static class ApplicationDbContextExtensions
{
    public static async Task EnableForeignKeysAsync(this ApplicationDbContext context)
    {
        if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
        {
            await context.Database.ExecuteSqlRawAsync("PRAGMA foreign_keys = ON;");
        }
    }
}
