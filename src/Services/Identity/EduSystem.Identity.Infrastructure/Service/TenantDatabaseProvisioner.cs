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

    //public async Task<string> CreateDatabaseAsync(string tenantSlug)
    //{
    //    // 1. Tenant-specific database name
    //    var dbName = $"EduSystem_{tenantSlug}";

    //    // 2. Parse master connection string
    //    var builder = new SqlConnectionStringBuilder(_masterConnectionString);

    //    // 3. Connect to 'master' database to create new database
    //    var originalDatabase = builder.InitialCatalog;
    //    builder.InitialCatalog = "master";
    //    var masterConnection = builder.ConnectionString;

    //    // 4. Create new database if not exists
    //    await using (var connection = new SqlConnection(masterConnection))
    //    {
    //        await connection.OpenAsync();

    //        // Check if database exists
    //        var checkDbQuery = $"SELECT database_id FROM sys.databases WHERE Name = '{dbName}'";
    //        await using var checkCmd = new SqlCommand(checkDbQuery, connection);
    //        var exists = await checkCmd.ExecuteScalarAsync();

    //        if (exists == null)
    //        {
    //            // Create new database
    //            var createDbQuery = $"CREATE DATABASE [{dbName}]";
    //            await using var createCmd = new SqlCommand(createDbQuery, connection);
    //            await createCmd.ExecuteNonQueryAsync();

    //            //Console.WriteLine($"Database '{dbName}' created successfully!");
    //        }
    //        else
    //        {
    //            //Console.WriteLine($"Database '{dbName}' already exists.");
    //        }
    //    }

    //    // 5. Create tenant-specific connection string
    //    builder.InitialCatalog = dbName;
    //    var tenantConnectionString = builder.ConnectionString;

    //    // 6. Create tables in the new tenant database using TenantDbContext
    //    // TODO: এখানে TenantDbContext ব্যবহার করতে হবে (School/Student tables এর জন্য)
    //    // এখনকার জন্য শুধু database create করছি

    //    return tenantConnectionString;
    //}

    //public async Task<string> CreateDatabaseAsync_v2(string tenantSlug)
    //{
    //    var dbName = $"EduSystem_{tenantSlug}";
    //    var dbUser = $"tenant_{tenantSlug}_user";
    //    var dbPassword = await GenerateSecurePassword();

    //    var builder = new SqlConnectionStringBuilder(_masterConnectionString);
    //    var orginalDatabase = builder.InitialCatalog;
    //    builder.InitialCatalog = "master";
    //    var masterConnection = builder.ConnectionString;

    //    await using (var connection = new SqlConnection(masterConnection))
    //    {
    //        await connection.OpenAsync();

    //        var checkDbQuery = $"SELECT database_id FROM sys.database WHERE = '{dbName}'";
    //        await using var checkCmd = new SqlCommand(checkDbQuery, connection);
    //        var exits = await checkCmd.ExecuteScalarAsync();

    //        if (exits is null)
    //        {
    //            var createDbQuery = $"CREATE DATABASE [{dbName}]";
    //            await using var createCmd = new SqlCommand(createDbQuery, connection);
    //            var count = await createCmd.ExecuteNonQueryAsync(); //new to update line not need to store value of affected row amount
    //        }
    //        else
    //            return $"Database '{dbName}' already exits";

    //        var createLoginQuery = $@"
    //            IF NOT EXITS (SELECT * FROM sys.server_principals WHERE name = '{dbUser}')
    //            BEGIN
    //                CREATE LOGIN [{dbUser}] WITH PASSWORD = '{dbPassword}';
    //            END";

    //        await using var loginCmd = new SqlCommand(createLoginQuery, connection);
    //        await loginCmd.ExecuteNonQueryAsync();

    //        connection.ChangeDatabase(dbName);

    //        var createUserQuery = $@"
    //            IF NOT EXITS (SELECT * FROM sys.database_principals WHERE name = {dbUser})
    //            BEGIN
    //                CREATE USER [{dbUser}] FOR LOGIN [{dbUser}];
    //                ALTER ROLE db_owner ADD MEMBER [{dbUser}]
    //            END";
    //        await using var userCmd = new SqlCommand(createUserQuery, connection);
    //        await userCmd.ExecuteNonQueryAsync();
    //    }

    //    // Tenant connection string
    //    builder.InitialCatalog = dbName;
    //    builder.UserID = dbUser;
    //    builder.Password = dbPassword;
    //    builder.IntegratedSecurity = false;
    //    var tenantConnectionString = builder.ConnectionString;

    //    var tenantsConnectionString = builder.ConnectionString;
    //    var isEncrypted  = _connectionStringEncryptor.Encrypt(tenantsConnectionString, out string encryptedConn);

    //    if(!isEncrypted)
    //        return"Failed to encrypt connection string";

    //    return encryptedConn;
    //}

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
    private async Task CreateDatabaseAsync(SqlConnection connection, string dbName)
    {
        var query = $"CREATE DATABASE [{dbName}]";
        await using var cmd = new SqlCommand(query, connection);
        await cmd.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Create login
    /// </summary>
    private async Task CreateLoginAsync(SqlConnection connection, string user, string password)
    {
        var query = $@"
        IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '{user}')
        BEGIN
            CREATE LOGIN [{user}] WITH PASSWORD = '{password}';
        END";

        await using var cmd = new SqlCommand(query, connection);
        await cmd.ExecuteNonQueryAsync();
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

    public async Task<string> CreateDatabaseAsync(string tenantSlug)
    {
        var dbName = $"EduSystem_{tenantSlug}";
        var dbUser = $"tenant_{tenantSlug}_user";
        var dbPassword = await GenerateSecurePassword();

        var masterConnection = GetMasterConnectionString();

        await using (var connection = new SqlConnection(masterConnection))
        {
            await connection.OpenAsync();

            if (!await DatabaseExistsAsync(connection, dbName))
            {
                await CreateDatabaseAsync(connection, dbName);
            }
            else
            {
                return $"Database '{dbName}' already exists";
            }

            await CreateLoginAsync(connection, dbUser, dbPassword);

            connection.ChangeDatabase(dbName);
            await CreateUserAndAssignRoleAsync(connection, dbUser);
        }

        var tenantConnectionString = BuildTenantConnectionString(dbName, dbUser, dbPassword);

        if (!_connectionStringEncryptor.Encrypt(tenantConnectionString, out string encrypted))
            return "Failed to encrypt connection string";

        return encrypted;
    }
}