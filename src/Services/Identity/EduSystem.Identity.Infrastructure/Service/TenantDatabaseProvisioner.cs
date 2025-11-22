using EduSystem.Identity.Application.IService;
using EduSystem.Shared.Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace EduSystem.Identity.Infrastructure.Service;

public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
{
    private readonly string _masterConnectionString;
    private readonly IConnectionStringEncryptor _connectionStringEncryptor;
    public TenantDatabaseProvisioner(IConfiguration configuration, IConnectionStringEncryptor connectionStringEncryptor)
    {
        _masterConnectionString = configuration.GetConnectionString("MasterDBConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "MasterDBConnection not found in appsettings.json");
        _connectionStringEncryptor = connectionStringEncryptor;
    }

    public async Task<bool> DropDatabaseAsync(string tenantSlug)
    {
        var databaseName = $"EduSystem_{tenantSlug}";

        try
        {
            using var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync();

            // Close all connections
            var closeConnections = $@"
            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
        ";

            using (var command = new SqlCommand(closeConnections, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            // Drop database
            var dropDatabase = $"DROP DATABASE [{databaseName}];";
            using (var command = new SqlCommand(dropDatabase, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            return true; // Success
        }
        catch (Exception ex)
        {
            // Optional: log the exception
            Console.WriteLine(ex.Message);
            return false; // Failure
        }
    }

    private async Task<string> GenerateSecurePassword()
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$?_-";
        var random = new Random();
        var password = new char[16];
        for(int i = 0; i < 16; i++)
        {
            password[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(password);
    }

    /// <summary>
    /// Build master connection string
    /// </summary>
    private string GetMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_masterConnectionString);
        builder.InitialCatalog = "master";
        return builder.ConnectionString;
    }

    /// <summary>
    /// Check if database exists
    /// </summary>
    private async Task<bool> DatabaseExistsAsync(SqlConnection connection, string dbName)
    {
        var query = "SELECT database_id FROM sys.databases WHERE name = @name";
        await using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@name", dbName);

        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    /// <summary>
    /// Create database
    /// </summary>
    private async Task<bool> CreateDatabaseAsync(SqlConnection connection, string dbName)
    {
        var query = $"CREATE DATABASE [{dbName}]";
        try
        {
            await using var cmd = new SqlCommand(query, connection);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }

    }

    /// <summary>
    /// Create login
    /// </summary>
    private async Task<bool> CreateLoginAsync(SqlConnection connection, string user, string password)
    {
        var query = $@"
        IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '{user}')
        BEGIN
            CREATE LOGIN [{user}] WITH PASSWORD = '{password}';
        END";

        try
        {
            await using var cmd = new SqlCommand(query, connection);
            await cmd.ExecuteNonQueryAsync();
            return true;   // success
        }
        catch (Exception)
        {
            // log error if you want
            return false;  // failed
        }
    }

    /// <summary>
    /// Create DB user + assign db_owner
    /// </summary>
    private async Task CreateUserAndAssignRoleAsync(SqlConnection connection, string user)
    {
        var query = $@"
        IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '{user}')
        BEGIN
            CREATE USER [{user}] FOR LOGIN [{user}];
            ALTER ROLE db_owner ADD MEMBER [{user}];
        END";

        await using var cmd = new SqlCommand(query, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Build tenant connection string
    /// </summary>
    private string BuildTenantConnectionString(string dbName, string user, string password)
    {
        var builder = new SqlConnectionStringBuilder(_masterConnectionString);
        builder.InitialCatalog = dbName;
        builder.UserID = user;
        builder.Password = password;
        builder.IntegratedSecurity = false;

        return builder.ConnectionString;
    }

    public async Task<(bool Success, string EncryptedConnectionString)> CreateDatabaseAsync(string tenantSlug)
    {
        var dbName = $"EduSystem_{tenantSlug}";
        var dbUser = $"tenant_{tenantSlug}_user";

        var dbPassword = await GenerateSecurePassword();

        if (string.IsNullOrWhiteSpace(dbPassword))
            return (false, "Failed to generate secure DB password");

        var masterConnection = GetMasterConnectionString();

        if (string.IsNullOrWhiteSpace(masterConnection))
            return (false, "Master connection string is invalid");

        await using (var connection = new SqlConnection(masterConnection))
        {
            await connection.OpenAsync();

            if (!await DatabaseExistsAsync(connection, dbName))
            {
                try
                {
                    await CreateDatabaseAsync(connection, dbName);
                }
                catch (Exception ex)
                {
                    if (await DatabaseExistsAsync(connection, dbName))
                        return (false, $"Database '{dbName}' already exists");
                    else
                        return (false, ex.StackTrace ?? "DB Creation Failed");
                }
            }
            else
            {
                return (false, $"Database '{dbName}' already exists");
            }

            var isLoginCreated = await CreateLoginAsync(connection, dbUser, dbPassword);

            if (!isLoginCreated)
                return (false, "Failed to create login");

            connection.ChangeDatabase(dbName);
            await CreateUserAndAssignRoleAsync(connection, dbUser);
        }

        var tenantConnectionString = BuildTenantConnectionString(dbName, dbUser, dbPassword);

        if (!_connectionStringEncryptor.Encrypt(tenantConnectionString, out string encrypted))
            return (false, "Failed to encrypt connection string");

        return (true, encrypted);
    }
}