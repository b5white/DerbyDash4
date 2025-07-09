using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data
{
    [Table("Log")]
    public class Log
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int LogLevel { get; set; }

        public int? ThreadId { get; set; }

        public int? EventId { get; set; }

        [MaxLength(100)]
        public string? EventName { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        [MaxLength(450)]
        public string? UserId { get; set; }

        public int? RacerId { get; set; }

        [MaxLength(100)]
        public string? SessionId { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(1000)]
        public string? ExceptionMessage { get; set; }

        public string? ExceptionStackTrace { get; set; }

        [MaxLength(100)]
        public string? ExceptionSource { get; set; }

        [Required]
        [Column(TypeName = "datetime2")]
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