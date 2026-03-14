using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Persistence;

public class ApplicationDbContextInitialiser(
    ApplicationDbContext context,
    ILogger<ApplicationDbContextInitialiser> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            // Ensure database is created and migrations are applied
            logger.LogInformation("Checking database connection...");
            
            if (await context.Database.CanConnectAsync())
            {
                logger.LogInformation("Database connection successful. Applying migrations...");
                await context.Database.MigrateAsync();
                logger.LogInformation("Migrations applied successfully.");
            }
            else
            {
                logger.LogWarning("Cannot connect to database. Creating database...");
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created successfully.");
            }

            // Check if data exists and seed if necessary
            await SeedDataIfNeededAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;
        }
    }

    private async Task SeedDataIfNeededAsync()
    {
        // Check if any users exist
        var hasUsers = await context.Users.AnyAsync();
        
        if (!hasUsers)
        {
            logger.LogInformation("No data found. Database seeding will be performed automatically via OnModelCreating.");
            logger.LogInformation("Triggering initial query to invoke seeding...");
            
            // This will trigger OnModelCreating which contains the seed data
            await context.SaveChangesAsync();
            
            logger.LogInformation("Database seeded successfully.");
        }
        else
        {
            logger.LogInformation("Database already contains data. Skipping seeding.");
        }
    }
}