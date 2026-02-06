using Microsoft.EntityFrameworkCore;
using FitnessTracker.BackEnd.Models;

namespace FitnessTracker.BackEnd.Data
{
    public class FitnessDbContext : DbContext
    {
        public FitnessDbContext(DbContextOptions<FitnessDbContext> options) : base(options)
        {
        }

        public DbSet<Activity> Activities => Set<Activity>();
        public DbSet<AppUser> Users => Set<AppUser>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}
