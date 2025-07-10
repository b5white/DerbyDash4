using DerbyDash.Data;

namespace DerbyDash.Services
{
    public interface ILogService
    {
        Task<List<Log>> GetLogsAsync(DateTime? fromDate = null, string? userId = null, int? racerId = null, int? logLevel = null, string? sessionId = null, int limit = 100);
        Task<Log?> GetLogByIdAsync(int id);
        Task<List<Log>> GetLogsByUserIdAsync(string userId, DateTime? fromDate = null, int limit = 100);
        Task<List<Log>> GetLogsByRacerIdAsync(int racerId, DateTime? fromDate = null, int limit = 100);
        Task<List<Log>> GetExceptionLogsAsync(DateTime? fromDate = null, int limit = 100);
    }
}