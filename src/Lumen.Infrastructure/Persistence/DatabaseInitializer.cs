using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Lumen.Infrastructure.Persistence;

/// <summary>
/// Applies EF Core migrations and handles legacy databases created before migrations were introduced.
/// </summary>
internal static class DatabaseInitializer
{
    public static async Task MigrateAsync(AppDbContext dbContext, IHostEnvironment? environment)
    {
        var isDevelopment = environment?.IsDevelopment() ?? true;
        var appliedMigrations = (await dbContext.Database.GetAppliedMigrationsAsync()).ToList();

        if (appliedMigrations.Count == 0 && await HasUserTablesAsync(dbContext))
        {
            if (!isDevelopment)
            {
                throw new InvalidOperationException(
                    "The database has tables but no EF migration history. " +
                    "This usually means it was created with EnsureCreated in an earlier phase. " +
                    "Delete the database file or baseline migrations before running in non-development environments.");
            }

            await dbContext.Database.EnsureDeletedAsync();
        }

        await dbContext.Database.MigrateAsync();
    }

    private static async Task<bool> HasUserTablesAsync(AppDbContext dbContext)
    {
        var connection = dbContext.Database.GetDbConnection();
        await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT COUNT(*) FROM sqlite_master
                WHERE type = 'table'
                  AND name NOT LIKE 'sqlite_%'
                  AND name <> '__EFMigrationsHistory'
                """;

            var count = await command.ExecuteScalarAsync();
            return Convert.ToInt32(count) > 0;
        }
        finally
        {
            await connection.CloseAsync();
        }
    }
}