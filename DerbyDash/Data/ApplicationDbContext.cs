using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using DerbyDash.Data;

namespace DerbyDash.Data {
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options): IdentityDbContext<ApplicationUser, IdentityRole, string>(options) {
    public DbSet<Racer> Racers { get; set; }
    public DbSet<Race> Races { get; set; }
    public DbSet<SpeedIncrement> SpeedIncrements { get; set; }
    }
}
