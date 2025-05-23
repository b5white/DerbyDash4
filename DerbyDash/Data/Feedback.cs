using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace DerbyDash.Data
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;

        public ApplicationUser User { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public FeedbackType FeedbackType { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Subject { get; set; } = string.Empty;
        
        [Required]
        [StringLength(2000)]
        public string Message { get; set; } = string.Empty;
        
        public string BrowserInfo { get; set; } = string.Empty;
        
        public bool ContactConsent { get; set; } = true;
        
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        
        public bool IsResolved { get; set; } = false;
        
        public string? AdminNotes { get; set; }
        
        public DateTime? ResolvedAt { get; set; }
    }
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum FeedbackType
    {
        BugReport,
        FeatureRequest,
        GeneralFeedback,
        Question
    }
}