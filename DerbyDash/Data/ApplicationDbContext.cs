using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Data {
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): IdentityDbContext<ApplicationUser, IdentityRole, string>(options) {
        public DbSet<Feedback> Feedbacks { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // No special configuration needed for Feedback entity
            // since we removed the User relationship
        }
    }
}
