using Microsoft.EntityFrameworkCore;
using ToDoApp.Infrastructure.DatabaseContext;

namespace ToDoApp.WebAPI.Extensions.Main;

public static class MigrationExtension
{
    public static async Task ApplyMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var retries = 5;
        for (var attempt = 1; attempt <= retries; attempt++)
        {
            try
            {
                logger.LogInformation("Poganjam EF Core migracije (poskus {Attempt}/{Retries})...", attempt, retries);
                await dbContext.Database.MigrateAsync();
                logger.LogInformation("Migracije uspešno izvedene.");
                break;
            }
            catch (Exception ex) when (attempt < retries)
            {
                logger.LogWarning(ex, "Migracija ni uspela, poskušam znova čez 3s...");
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }
    }
}
