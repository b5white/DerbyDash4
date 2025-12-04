namespace DerbyDash.Data {
    public class Subscription {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public SubscriptionType Type { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool AutoRenewal { get; set; }
        public int ReferralCredits { get; set; }
        public bool IsPaused { get; set; }
        public DateTime? PausedDate { get; set; }
        public SubscriptionStatus Status { get; set; }
        public int SpecialOffersProgress { get; set; } // Percentage value (0-100)

        // Calculated properties
        public int DaysRemaining => (EndDate - DateTime.Now).Days > 0 ? (EndDate - DateTime.Now).Days : 0;
        public int ProgressPercentage =>
            (EndDate - StartDate).TotalDays == 0
                ? 100
                : 100 - (int)((double)DaysRemaining / (EndDate - StartDate).TotalDays * 100);
        public bool IsActive => Status == SubscriptionStatus.Active && !IsPaused && DateTime.Now <= EndDate;
        public bool IsExpired => DateTime.Now > EndDate && Status != SubscriptionStatus.Cancelled;
        public bool IsTrial => Type == SubscriptionType.Trial;
    }

    public enum SubscriptionType {
        Trial,
        Monthly,
        Annual,
        Lifetime
    }

    public enum SubscriptionStatus {
        Active,
        Expired,
        Cancelled,
        PendingRenewal
    }
}

