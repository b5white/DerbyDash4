using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DerbyDash.Data {
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): IdentityDbContext<ApplicationUser, IdentityRole, string>(options) {
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Racer> Racers { get; set; }
        public DbSet<Race> Races { get; set; }
        public DbSet<SpeedIncrement> SpeedIncrements { get; set; }
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure Racer relationship with ApplicationUser
            builder.Entity<Racer>()
                .HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure Race relationship with Racer
            builder.Entity<Race>()
                .HasOne<Racer>()
                .WithMany()
                .HasForeignKey(r => r.RacerId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Configure SpeedIncrement relationship with Race
            builder.Entity<SpeedIncrement>()
                .HasOne<Race>()
                .WithMany(r => r.SpeedIncrements)
                .HasForeignKey(si => si.RaceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
