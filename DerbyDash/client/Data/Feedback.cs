namespace DerbyDash.Data {
    // Client-side Feedback model (without EF attributes)
    public class Feedback {
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
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public bool IsResolved { get; set; } = false;
        public string? AdminNotes { get; set; }
        public string? PublicResponse { get; set; }
        public string? PrivateResponse { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }

    public enum FeedbackType {
        None = 0,
        BugReport,
        FeatureRequest,
        GeneralFeedback,
        Question
    }
}

