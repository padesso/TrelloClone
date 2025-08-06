using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace TrelloClone.Data
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
             using(var scope = host.Services.CreateScope())
             {
                 var logger = scope.ServiceProvider.GetService<ILogger<TrelloCloneDbContext>>();
                 using(var appContext = scope.ServiceProvider
                    .GetRequiredService<TrelloCloneDbContext>())
                 {
                    try
                    {
                        logger?.LogInformation("Checking database existence and applying migrations...");
                        
                        // Check if database exists
                        var canConnect = appContext.Database.CanConnect();
                        logger?.LogInformation($"Database can connect: {canConnect}");
                        
                        // Get pending migrations
                        var pendingMigrations = appContext.Database.GetPendingMigrations().ToList();
                        var appliedMigrations = appContext.Database.GetAppliedMigrations().ToList();
                        
                        logger?.LogInformation($"Applied migrations count: {appliedMigrations.Count}");
                        logger?.LogInformation($"Pending migrations count: {pendingMigrations.Count}");
                        
                        if (!canConnect || (!appliedMigrations.Any() && !pendingMigrations.Any()))
                        {
                            // No migrations exist, use EnsureCreated to create database and tables
                            logger?.LogInformation("No migrations found. Creating database using EnsureCreated...");
                            var created = appContext.Database.EnsureCreated();
                            logger?.LogInformation($"Database created: {created}");
                        }
                        else if (pendingMigrations.Any())
                        {
                            // Migrations exist, apply them
                            logger?.LogInformation("Applying pending migrations...");
                            appContext.Database.Migrate();
                            logger?.LogInformation("Migrations applied successfully.");
                        }
                        else
                        {
                            logger?.LogInformation("Database is up to date.");
                        }
                    }
                    catch (Exception ex)
                    {
                        logger?.LogError(ex, "An error occurred while initializing the database.");
                        throw;
                    }
                 }
             }

            return host;
        }
    }
}
