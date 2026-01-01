using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EduSystem.ApplicationUsers.Infrastructure.Contexts
{
    public class AppUserDbContextFactory : IDesignTimeDbContextFactory<AppUserDbContext>
    {
        public AppUserDbContext CreateDbContext(string[] args)
        {
            var basePath = AppContext.BaseDirectory;
            var apiPath = Path.Combine(basePath, "..", "EduSystem.ApplicationUsers.Api");

            if (!Directory.Exists(apiPath))
                apiPath = Path.Combine(basePath);

            var configuration = new ConfigurationBuilder()
                .SetBasePath(apiPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.Development.json", optional: true)
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<AppUserDbContext>();
            var connectionString = configuration.GetConnectionString("MasterDBConnection");


            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException(
               $"Connection string 'MasterDBConnection' not found. Searched in: {apiPath}");
            }

            optionsBuilder.UseSqlServer(connectionString);

            return new AppUserDbContext(optionsBuilder.Options);
        }
    }
}
