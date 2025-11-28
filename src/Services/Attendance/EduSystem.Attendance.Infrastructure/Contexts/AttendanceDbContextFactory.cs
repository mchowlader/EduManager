using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EduSystem.Attendance.Infrastructure.Contexts;

public class AttendanceDbContextFactory : IDesignTimeDbContextFactory<AttendanceDbContext>
{
    public AttendanceDbContext CreateDbContext(string[] args)
    {
        var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("MasterDBConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "MasterDBConnection not found. Ensure it is set in the API's appsettings.json file for design-time use.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<AttendanceDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AttendanceDbContext(optionsBuilder.Options);
    }
}
