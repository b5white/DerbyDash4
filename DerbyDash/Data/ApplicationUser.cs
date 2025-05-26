using Microsoft.AspNetCore.Identity;

namespace DerbyDash.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser: IdentityUser {
        ///// <summary>
        ///// Avatar image file name (e.g., "1.jpg", "2.jpg", etc.)
        ///// </summary>
        //[MaxLength(255)]
        //public string? AvatarFileName { get; set; }
    }
}
