namespace DerbyDash.Services {
    public class DatabaseKeepAliveService {
        private ILogger<DatabaseKeepAliveService> Logger;
        private static DateTime _lastCleanup = DateTime.MinValue;
        private static readonly object _lock = new object();

        public DatabaseKeepAliveService(ILogger<DatabaseKeepAliveService> logger) {
            Logger = logger;
        }

        public async Task PingDatabaseAsync() {
            int count = 0;
            bool success = false;
            int maxRetries = 15;
            if (Needed()) {
                Logger.LogInformation("Starting Database ping");
                while (!success && count < maxRetries) {
                    if (count > 0) {
                        Logger.LogWarning($"Retrying database ping... Attempt {count + 1} of {maxRetries}");
                        Console.WriteLine($"Retrying database ping... Attempt {count + 1} of {maxRetries}");
                    }
                    try {
                        //await Context.Database.ExecuteSqlRawAsync("SELECT 1");
                        Logger.LogInformation("Database ping successful");
                        success = true;
                    } catch (Exception ex) {
                        // An exception is expected. No value in logging it.
                        Logger.LogError($"Database ping failed, count={count}");
                        count++;
                        await Task.Delay(100); // Non-blocking delay
                    }
                }
            }
        }

        public static bool Needed() {
            // Guard clause: exit if not enough time has passed
            lock (_lock) {
                if ((DateTime.UtcNow - _lastCleanup) < TimeSpan.FromMinutes(30))
                    return false;

                // Update the last cleanup time before running cleanup
                _lastCleanup = DateTime.UtcNow;
                return true;
            }
        }
    }
}
