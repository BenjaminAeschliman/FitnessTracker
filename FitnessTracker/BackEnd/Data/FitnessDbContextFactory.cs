using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FitnessTracker.BackEnd.Data
{
    public class FitnessDbContextFactory : IDesignTimeDbContextFactory<FitnessDbContext>
    {
        public FitnessDbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<FitnessDbContext>();
            optionsBuilder.UseSqlite(config.GetConnectionString("FitnessDb"));

            return new FitnessDbContext(optionsBuilder.Options);
        }
    }
}
