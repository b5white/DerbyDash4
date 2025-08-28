using System.ComponentModel.DataAnnotations;
using DerbyDash.Data;

namespace DerbyDash.DTOs
{
    public class FeedbackDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int? RacerId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public FeedbackType FeedbackType { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string BrowserInfo { get; set; } = string.Empty;
        public bool ContactConsent { get; set; } = true;
        public DateTime SubmittedAt { get; set; }
        public bool IsResolved { get; set; } = false;
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }

        // For API compatibility with simpler models
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateFeedbackDto
    {
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

        // For API compatibility
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    public class UpdateFeedbackDto
    {
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        public FeedbackType FeedbackType { get; set; }

        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;

        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;

        [StringLength(500)]
        public string BrowserInfo { get; set; } = string.Empty;

        public bool ContactConsent { get; set; } = true;

        public bool IsResolved { get; set; } = false;

        [StringLength(1000)]
        public string? AdminNotes { get; set; }

        // For API compatibility
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
