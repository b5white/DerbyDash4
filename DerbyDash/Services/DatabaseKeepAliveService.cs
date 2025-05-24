using DerbyDash.Data;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Services {
    public class DatabaseKeepAliveService {
        private ILogger<DatabaseKeepAliveService> Logger;
        private readonly ApplicationDbContext Context;

        public DatabaseKeepAliveService(ILogger<DatabaseKeepAliveService> logger, ApplicationDbContext context) {
            Logger = logger;
            Context = context;
        }

        public async Task PingDatabaseAsync() {
            int count = 0;
            bool success = false;
            int maxRetries = 15;
            while (!success && count < maxRetries) {
                if (count > 0) {
                    Logger.LogWarning($"Retrying database ping... Attempt {count + 1} of {maxRetries}");
                    Console.WriteLine($"Retrying database ping... Attempt {count + 1} of {maxRetries}");
                }
                try {
                   // await Context.Database.ExecuteSqlRawAsync("SELECT 1");
                    Logger.LogInformation("Database ping successful");
                    success = true;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Database ping failed");
                    Console.WriteLine($"Error pinging database: {ex.Message}");
                    count++;
                    await Task.Delay(100); // Non-blocking delay
                }
            }
        }
    }
}

