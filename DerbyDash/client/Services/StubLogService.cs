using DerbyDash.Data;

namespace DerbyDash.Services {
    public class StubLogService : ILogService {
        public Task<List<Log>> GetLogsAsync(DateTime? fromDate = null, string? userId = null, int? racerId = null, int? logLevel = null, string? sessionId = null, int limit = 100) {
            // TODO: Implement API call to get logs
            return Task.FromResult(new List<Log>());
        }

        public Task<Log?> GetLogByIdAsync(int id) {
            // TODO: Implement API call to get log by ID
            return Task.FromResult<Log?>(null);
        }

        public Task<List<Log>> GetLogsByUserIdAsync(string userId, DateTime? fromDate = null, int limit = 100) {
            // TODO: Implement API call to get logs by user ID
            return Task.FromResult(new List<Log>());
        }

        public Task<List<Log>> GetLogsByRacerIdAsync(int racerId, DateTime? fromDate = null, int limit = 100) {
            // TODO: Implement API call to get logs by racer ID
            return Task.FromResult(new List<Log>());
        }

        public Task<List<Log>> GetExceptionLogsAsync(DateTime? fromDate = null, int limit = 100) {
            // TODO: Implement API call to get exception logs
            return Task.FromResult(new List<Log>());
        }

        public Task LogAsync(string level, string message, Exception? exception = null) {
            // TODO: Implement API call to log
            return Task.CompletedTask;
        }
    }
}
