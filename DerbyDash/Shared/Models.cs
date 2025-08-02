using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Shared.Models {
    public class RacerDto {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public DateOnly? LastRaced { get; set; } = null;
        public string? LastPlayedRace { get; set; }
        public string? AvatarFileName { get; set; }
        public int RaceCount { get; set; }
    }

    public class RaceDto {
        public int Id { get; set; }
        public int RacerId { get; set; }
        public DateTime RaceDateTime { get; set; } = DateTime.Now;
        public double TotalTime { get; set; }
        public int ProblemSetId { get; set; }
        public int ImageId { get; set; }
        public int FinishingPosition { get; set; } = 0;
        public List<SpeedIncrementDto>? SpeedIncrements { get; set; }
    }

    public class SpeedIncrementDto {
        public int Id { get; set; }
        public int RaceId { get; set; }
        public double Time { get; set; }
        public double Speed { get; set; }
        public double Distance { get; set; }
    }

    public class FeedbackDto {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? RacerId { get; set; }
        public string? SessionId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? UserAgent { get; set; }
        public string? IPAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool CanReply { get; set; } = false;
        public string? ContactEmail { get; set; }
        public string? Response { get; set; }
        public DateTime? ResponseDate { get; set; }
    }

    public class LogDto {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
        public string Level { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Exception { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Source { get; set; }
    }
}
