namespace DerbyDash.Data {
    // Client-side Log model
    public class Log {
        public int Id { get; set; }
        public int LogLevel { get; set; }
        public int? ThreadId { get; set; }
        public int? EventId { get; set; }
        public string? EventName { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public int? RacerId { get; set; }
        public string? SessionId { get; set; }
        public string? Category { get; set; }
        public string? ExceptionMessage { get; set; }
        public string? ExceptionStackTrace { get; set; }
        public string? ExceptionSource { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AppLogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5
    }
}
