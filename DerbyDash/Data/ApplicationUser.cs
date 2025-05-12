using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace DerbyDash.Data {
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser: IdentityUser {
        [Column("ActiveFamilyMemberId")]
        public string? ActiveRacerId { get; set; }
    }
}
