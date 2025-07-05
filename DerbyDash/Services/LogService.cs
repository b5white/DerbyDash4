using Bogus;
using DerbyDash.Data;

namespace DerbyDash.Services
{
    public class LogService : ILogService
    {
        private readonly ILogger<LogService> _logger;
        private List<Log> Logs;

        public LogService(ILogger<LogService> logger)
        {
            _logger = logger;
            Logs = GenerateFakeLogs(50); // Generate some sample logs
        }

        public async Task<List<Log>> GetLogsAsync(DateTime? fromDate = null, string? userId = null, int? racerId = null, int? logLevel = null, int limit = 100)
        {
            var query = Logs.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);

            if (!string.IsNullOrEmpty(userId))
                query = query.Where(l => l.UserId == userId);

            if (racerId.HasValue)
                query = query.Where(l => l.RacerId == racerId.Value);

            if (logLevel.HasValue)
                query = query.Where(l => l.LogLevel >= logLevel.Value);

            var result = query
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToList();

            return await Task.FromResult(result);
        }

        public async Task<Log?> GetLogByIdAsync(int id)
        {
            var result = Logs.FirstOrDefault(l => l.Id == id);
            return await Task.FromResult(result);
        }

        public async Task<List<Log>> GetLogsByUserIdAsync(string userId, DateTime? fromDate = null, int limit = 100)
        {
            return await GetLogsAsync(fromDate, userId, null, null, limit);
        }

        public async Task<List<Log>> GetLogsByRacerIdAsync(int racerId, DateTime? fromDate = null, int limit = 100)
        {
            return await GetLogsAsync(fromDate, null, racerId, null, limit);
        }

        public async Task<List<Log>> GetExceptionLogsAsync(DateTime? fromDate = null, int limit = 100)
        {
            var query = Logs.AsQueryable()
                .Where(l => !string.IsNullOrEmpty(l.ExceptionMessage));

            if (fromDate.HasValue)
                query = query.Where(l => l.CreatedAt >= fromDate.Value);

            var result = query
                .OrderByDescending(l => l.CreatedAt)
                .Take(limit)
                .ToList();

            return await Task.FromResult(result);
        }

        private List<Log> GenerateFakeLogs(int count = 50)
        {
            var logLevels = new[] { 0, 1, 2, 3, 4, 5 }; // Trace, Debug, Info, Warning, Error, Critical
            var categories = new[] { "System", "Authentication", "Race", "Feedback", "Exception", "Database" };
            var eventNames = new[] { "UserLogin", "RaceStart", "FeedbackSubmitted", "DatabaseConnection", "Exception", "UserRegistration" };
            
            var faker = new Faker<Log>()
                .RuleFor(l => l.Id, f => f.IndexFaker + 1)
                .RuleFor(l => l.LogLevel, f => f.PickRandom(logLevels))
                .RuleFor(l => l.ThreadId, f => f.Random.Int(1, 10))
                .RuleFor(l => l.EventId, f => f.Random.Int(1000, 9999))
                .RuleFor(l => l.EventName, f => f.PickRandom(eventNames))
                .RuleFor(l => l.Message, f => f.Lorem.Sentence(8))
                .RuleFor(l => l.UserId, f => f.Random.Bool(0.6f) ? f.Random.Guid().ToString() : null)
                .RuleFor(l => l.RacerId, f => f.Random.Bool(0.4f) ? f.Random.Int(1, 100) : null)
                .RuleFor(l => l.Category, f => f.PickRandom(categories))
                .RuleFor(l => l.CreatedAt, f => f.Date.Recent(30));

            // Generate logs with some exceptions
            var logs = faker.Generate(count);
            
            // Add some exception details to error logs
            foreach (var log in logs.Where(l => l.LogLevel >= 4)) // Error and Critical
            {
                if (new Random().NextDouble() > 0.5) // 50% chance to have exception details
                {
                    log.ExceptionMessage = new Faker().Lorem.Sentence(6);
                    log.ExceptionSource = new Faker().PickRandom(new[] { "DerbyDash.Services", "DerbyDash.Controllers", "DerbyDash.Data", "System.Data" });
                    log.ExceptionStackTrace = $"   at {log.ExceptionSource}.SomeMethod() in /path/to/file.cs:line {new Random().Next(10, 200)}\n" +
                                            $"   at DerbyDash.Program.Main() in /path/to/Program.cs:line {new Random().Next(10, 50)}";
                }
            }

            return logs.OrderByDescending(l => l.CreatedAt).ToList();
        }
    }
}