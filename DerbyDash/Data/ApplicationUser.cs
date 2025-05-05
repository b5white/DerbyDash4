using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser: IdentityUser {
        // Avatar image file name (e.g., "1.png", "2.png", "3.png")
        public string? AvatarFileName { get; set; }
    }

}
