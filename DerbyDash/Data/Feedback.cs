using System;
using System.ComponentModel.DataAnnotations;

namespace DerbyDash.Data
{
    public class Feedback
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string FeedbackType { get; set; } = string.Empty;
        
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
}