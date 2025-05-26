using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    [Index(nameof(UserId), nameof(Status))]
    public class Subscription {
        public int Id { get; set; }

        [ForeignKey("User")]
        public required string UserId { get; set; }  // Foreign key to AspNetUsers
        [Required]
        public ApplicationUser? User { get; set; }  // Navigation property

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
        [NotMapped]
        public int DaysRemaining => (EndDate - DateTime.Now).Days > 0 ? (EndDate - DateTime.Now).Days : 0;
        [NotMapped]
        public int ProgressPercentage => 100 - (int)((double)DaysRemaining / (EndDate - StartDate).TotalDays * 100);
        [NotMapped]
        public bool IsActive => Status == SubscriptionStatus.Active && !IsPaused && DateTime.Now <= EndDate;
        [NotMapped]
        public bool IsExpired => DateTime.Now > EndDate && Status != SubscriptionStatus.Cancelled;
        [NotMapped]
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