using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ToTraveler.Auth.Model;
using ToTraveler.Models;

namespace ToTraveler
{
    public class AppDbContext : IdentityDbContext<User>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<Location> Locations { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<LocationCategory> LocationCategories { get; set; }
        public DbSet<Review> Reviews { get; set; }
    }
}
