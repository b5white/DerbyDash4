using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser: IdentityUser {
        /// <summary>
        /// Maximum number of racers allowed for this user. If null, uses subscription-based default.
        /// </summary>
        public int? RacerLimit { get; set; }
        
        /// <summary>
        /// Whether this user's racer limit has been manually set by an admin
        /// </summary>
        public bool IsRacerLimitOverridden { get; set; } = false;
    }
}
