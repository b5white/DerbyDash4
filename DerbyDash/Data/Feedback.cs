using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace DerbyDash.Data {
    public class Feedback {
        [Key]
        public int Id { get; set; }

        [StringLength(450)]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public int? RacerId { get; set; }

        [ForeignKey("RacerId")]
        public Racer? Racer { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public FeedbackType FeedbackType { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        public string BrowserInfo { get; set; } = string.Empty;

        public bool ContactConsent { get; set; } = true;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public bool IsResolved { get; set; } = false;

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        public DateTime? ResolvedAt { get; set; }
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FeedbackType {
        None = 0,
        BugReport,
        FeatureRequest,
        GeneralFeedback,
        Question
    }
}