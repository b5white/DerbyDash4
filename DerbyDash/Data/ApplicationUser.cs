using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser: IdentityUser {
        [Column("ActiveFamilyMemberId")]
        public int? ActiveRacerId { get; set; }
        // Avatar image file name (e.g., "1.png", "2.png", "3.png")
        public string? AvatarFileName { get; set; }
        // Last played race identifier (e.g., "addition-4stable")
        public string? LastPlayedRace { get; set; }
        // Last time the user played a race
        public DateTime? LastPlayedTime { get; set; }
        // Total number of races played by all team members
        public int TeamRaceCount { get; set; } = 0;
    }
}
