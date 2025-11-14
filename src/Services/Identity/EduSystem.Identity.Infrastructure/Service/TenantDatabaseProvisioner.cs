
using EduSystem.Identity.Application.IService;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSystem.Identity.Infrastructure.Service;

public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
{
    private readonly string _masterConnectionString;

    public TenantDatabaseProvisioner(IConfiguration configuration)
    {
        _masterConnectionString = configuration.GetConnectionString("MasterDBConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "MasterDBConnection not found in appsettings.json");
    }

    public async Task<string> CreateDatabaseAsync(string tenantSlug)
    {
        // 1. Tenant-specific database name
        var dbName = $"EduSystem_{tenantSlug}";

        // 2. Parse master connection string
        var builder = new SqlConnectionStringBuilder(_masterConnectionString);

        // 3. Connect to 'master' database to create new database
        var originalDatabase = builder.InitialCatalog;
        builder.InitialCatalog = "master";
        var masterConnection = builder.ConnectionString;

        // 4. Create new database if not exists
        await using (var connection = new SqlConnection(masterConnection))
        {
            await connection.OpenAsync();

            // Check if database exists
            var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE Name = '{dbName}'";
            await using var checkCmd = new SqlCommand(checkDbQuery, connection);
            var exists = await checkCmd.ExecuteScalarAsync();

            if (exists == null)
            {
                // Create new database
                var createDbQuery = $"CREATE DATABASE [{dbName}]";
                await using var createCmd = new SqlCommand(createDbQuery, connection);
                await createCmd.ExecuteNonQueryAsync();

                Console.WriteLine($"Database '{dbName}' created successfully!");
            }
            else
            {
                Console.WriteLine($"Database '{dbName}' already exists.");
            }
        }

        // 5. Create tenant-specific connection string
        builder.InitialCatalog = dbName;
        var tenantConnectionString = builder.ConnectionString;

        // 6. Create tables in the new tenant database using TenantDbContext
        // TODO: এখানে TenantDbContext ব্যবহার করতে হবে (School/Student tables এর জন্য)
        // এখনকার জন্য শুধু database create করছি

        return tenantConnectionString;
    }

    public async Task DropDatabaseAsync(string tenantSlug)
    {
        var databaseName = $"EduSystem_{tenantSlug}";

        using var connection = new SqlConnection(_masterConnectionString);
        await connection.OpenAsync();

        // all connection close 
        var closeConnections = $@"
        ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    ";

        using (var command = new SqlCommand(closeConnections, connection))
        {
            await command.ExecuteNonQueryAsync();
        }

        // Database drop 
        var dropDatabase = $"DROP DATABASE [{databaseName}];";
        using (var command = new SqlCommand(dropDatabase, connection))
        {
            await command.ExecuteNonQueryAsync();
        }
    }

}