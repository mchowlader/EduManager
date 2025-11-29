//using EduSystem.Identity.Application.IService;
//using EduSystem.Shared.Infrastructure.Security;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Configuration;

//namespace EduSystem.Identity.Infrastructure.Service;

//public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
//{
//    private readonly string _masterConnectionString;
//    private readonly IConnectionStringEncryptor _connectionStringEncryptor;
//    public TenantDatabaseProvisioner(IConfiguration configuration, IConnectionStringEncryptor connectionStringEncryptor)
//    {
//        _masterConnectionString = configuration.GetConnectionString("MasterDBConnection")
//            ?? throw new ArgumentNullException(nameof(configuration), "MasterDBConnection not found in appsettings.json");
//        _connectionStringEncryptor = connectionStringEncryptor;
//    }

//    public async Task<bool> DropDatabaseAsync(string tenantSlug)
//    {
//        var databaseName = $"EduSystem_{tenantSlug}";

//        try
//        {
//            using var connection = new SqlConnection(_masterConnectionString);
//            await connection.OpenAsync();

//            // Close all connections
//            var closeConnections = $@"
//            ALTER DATABASE [{databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
//        ";

//            using (var command = new SqlCommand(closeConnections, connection))
//            {
//                await command.ExecuteNonQueryAsync();
//            }

//            // Drop database
//            var dropDatabase = $"DROP DATABASE [{databaseName}];";
//            using (var command = new SqlCommand(dropDatabase, connection))
//            {
//                await command.ExecuteNonQueryAsync();
//            }

//            return true; // Success
//        }
//        catch (Exception ex)
//        {
//            // Optional: log the exception
//            Console.WriteLine(ex.Message);
//            return false; // Failure
//        }
//    }

//    private async Task<string> GenerateSecurePassword()
//    {
//        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$?_-";
//        var random = new Random();
//        var password = new char[16];
//        for(int i = 0; i < 16; i++)
//        {
//            password[i] = validChars[random.Next(validChars.Length)];
//        }

//        return new string(password);
//    }

//    /// <summary>
//    /// Build master connection string
//    /// </summary>
//    private string GetMasterConnectionString()
//    {
//        var builder = new SqlConnectionStringBuilder(_masterConnectionString);
//        builder.InitialCatalog = "master";
//        return builder.ConnectionString;
//    }

//    /// <summary>
//    /// Check if database exists
//    /// </summary>
//    private async Task<bool> DatabaseExistsAsync(SqlConnection connection, string dbName)
//    {
//        var query = "SELECT database_id FROM sys.databases WHERE name = @name";
//        await using var cmd = new SqlCommand(query, connection);
//        cmd.Parameters.AddWithValue("@name", dbName);

//        var result = await cmd.ExecuteScalarAsync();
//        return result != null;
//    }

//    /// <summary>
//    /// Create database
//    /// </summary>
//    private async Task<bool> CreateDatabaseAsync(SqlConnection connection, string dbName)
//    {
//        var query = $"CREATE DATABASE [{dbName}]";
//        try
//        {
//            await using var cmd = new SqlCommand(query, connection);
//            await cmd.ExecuteNonQueryAsync();
//            return true;
//        }
//        catch (Exception)
//        {
//            return false;
//        }

//    }

//    /// <summary>
//    /// Create login
//    /// </summary>
//    private async Task<bool> CreateLoginAsync(SqlConnection connection, string user, string password)
//    {
//        var query = $@"
//        IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = '{user}')
//        BEGIN
//            CREATE LOGIN [{user}] WITH PASSWORD = '{password}';
//        END";

//        try
//        {
//            await using var cmd = new SqlCommand(query, connection);
//            await cmd.ExecuteNonQueryAsync();
//            return true;   // success
//        }
//        catch (Exception)
//        {
//            // log error if you want
//            return false;  // failed
//        }
//    }

//    /// <summary>
//    /// Create DB user + assign db_owner
//    /// </summary>
//    private async Task CreateUserAndAssignRoleAsync(SqlConnection connection, string user)
//    {
//        var query = $@"
//        IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = '{user}')
//        BEGIN
//            CREATE USER [{user}] FOR LOGIN [{user}];
//            ALTER ROLE db_owner ADD MEMBER [{user}];
//        END";

//        await using var cmd = new SqlCommand(query, connection);
//        await cmd.ExecuteNonQueryAsync();
//    }

//    /// <summary>
//    /// Build tenant connection string
//    /// </summary>
//    private string BuildTenantConnectionString(string dbName, string user, string password)
//    {
//        var builder = new SqlConnectionStringBuilder(_masterConnectionString);
//        builder.InitialCatalog = dbName;
//        builder.UserID = user;
//        builder.Password = password;
//        builder.IntegratedSecurity = false;

//        return builder.ConnectionString;
//    }

//    public async Task<(bool Success, string EncryptedConnectionString)> CreateDatabaseAsync(string tenantSlug)
//    {
//        var dbName = $"EduSystem_{tenantSlug}";
//        var dbUser = $"tenant_{tenantSlug}_user";

//        var dbPassword = await GenerateSecurePassword();

//        if (string.IsNullOrWhiteSpace(dbPassword))
//            return (false, "Failed to generate secure DB password");

//        var masterConnection = GetMasterConnectionString();

//        if (string.IsNullOrWhiteSpace(masterConnection))
//            return (false, "Master connection string is invalid");

//        await using (var connection = new SqlConnection(masterConnection))
//        {
//            await connection.OpenAsync();

//            if (!await DatabaseExistsAsync(connection, dbName))
//            {
//                try
//                {
//                    await CreateDatabaseAsync(connection, dbName);
//                }
//                catch (Exception ex)
//                {
//                    if (await DatabaseExistsAsync(connection, dbName))
//                        return (false, $"Database '{dbName}' already exists");
//                    else
//                        return (false, ex.StackTrace ?? "DB Creation Failed");
//                }
//            }
//            else
//            {
//                return (false, $"Database '{dbName}' already exists");
//            }

//            var isLoginCreated = await CreateLoginAsync(connection, dbUser, dbPassword);

//            if (!isLoginCreated)
//                return (false, "Failed to create login");

//            connection.ChangeDatabase(dbName);
//            await CreateUserAndAssignRoleAsync(connection, dbUser);
//        }

//        var tenantConnectionString = BuildTenantConnectionString(dbName, dbUser, dbPassword);

//        if (!_connectionStringEncryptor.Encrypt(tenantConnectionString, out string encrypted))
//            return (false, "Failed to encrypt connection string");

//        return (true, encrypted);
//    }
//}

using EduSystem.Identity.Application.IService;
using EduSystem.Shared.Infrastructure.Security;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EduSystem.Identity.Infrastructure.Service;

public class TenantDatabaseProvisioner : ITenantDatabaseProvisioner
{
    private readonly string _masterConnectionString;
    private readonly IConnectionStringEncryptor _connectionStringEncryptor;
    private readonly ILogger<TenantDatabaseProvisioner> _logger;

    public TenantDatabaseProvisioner(
        IConfiguration configuration,
        IConnectionStringEncryptor connectionStringEncryptor,
        ILogger<TenantDatabaseProvisioner> logger)
    {
        _masterConnectionString = configuration.GetConnectionString("MasterDBConnection")
            ?? throw new ArgumentNullException(nameof(configuration), "MasterDBConnection not found in appsettings.json");
        _connectionStringEncryptor = connectionStringEncryptor;
        _logger = logger;
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

        try
        {
            _logger.LogInformation($"🔧 Starting database provisioning for tenant: {tenantSlug}");

            await using (var connection = new SqlConnection(masterConnection))
            {
                await connection.OpenAsync();

                // Step 1: Check if database exists
                if (await DatabaseExistsAsync(connection, dbName))
                {
                    _logger.LogWarning($"⚠️ Database '{dbName}' already exists");
                    return (false, $"Database '{dbName}' already exists");
                }

                // Step 2: Create database
                _logger.LogInformation($"📦 Creating database: {dbName}");
                if (!await CreateDatabaseAsync(connection, dbName))
                {
                    _logger.LogError($"❌ Failed to create database: {dbName}");
                    return (false, $"Failed to create database '{dbName}'");
                }
                _logger.LogInformation($"✅ Database created: {dbName}");

                // Step 3: Create login
                _logger.LogInformation($"👤 Creating login: {dbUser}");
                if (!await CreateLoginAsync(connection, dbUser, dbPassword))
                {
                    _logger.LogError($"❌ Failed to create login: {dbUser}");
                    await CleanupDatabaseAsync(connection, dbName);
                    return (false, "Failed to create login");
                }
                _logger.LogInformation($"✅ Login created: {dbUser}");

                // Step 4: Switch to tenant database and create user
                _logger.LogInformation($"🔐 Creating user and assigning permissions in: {dbName}");
                connection.ChangeDatabase(dbName);
                await CreateUserAndAssignRoleAsync(connection, dbUser);
                _logger.LogInformation($"✅ User created and permissions assigned");
            }

            // Step 5: Build tenant connection string
            var tenantConnectionString = BuildTenantConnectionString(dbName, dbUser, dbPassword);

            // Step 6: CRITICAL - Verify the connection works!
            _logger.LogInformation($"🔍 Verifying tenant connection...");
            var verificationResult = await VerifyTenantConnectionAsync(tenantConnectionString, dbName);

            if (!verificationResult.Success)
            {
                _logger.LogError($"❌ Connection verification failed: {verificationResult.Error}");

                // Cleanup on verification failure
                await using (var connection = new SqlConnection(masterConnection))
                {
                    await connection.OpenAsync();
                    await CleanupDatabaseAsync(connection, dbName);
                    await CleanupLoginAsync(connection, dbUser);
                }

                return (false, $"Connection verification failed: {verificationResult.Error}");
            }

            _logger.LogInformation($"✅ Connection verified successfully");

            // Step 7: Encrypt connection string
            if (!_connectionStringEncryptor.Encrypt(tenantConnectionString, out string encrypted))
            {
                _logger.LogError($"❌ Failed to encrypt connection string");
                return (false, "Failed to encrypt connection string");
            }

            _logger.LogInformation($"🎉 Database provisioning completed successfully for: {tenantSlug}");

            return (true, encrypted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Database provisioning failed for: {tenantSlug}");
            return (false, ex.Message);
        }
    }

    public async Task<bool> DropDatabaseAsync(string tenantSlug)
    {
        var databaseName = $"EduSystem_{tenantSlug}";
        var dbUser = $"tenant_{tenantSlug}_user";

        try
        {
            _logger.LogInformation($"🗑️ Dropping database: {databaseName}");

            using var connection = new SqlConnection(_masterConnectionString);
            await connection.OpenAsync();

            // Drop database
            await CleanupDatabaseAsync(connection, databaseName);

            // Drop login
            await CleanupLoginAsync(connection, dbUser);

            _logger.LogInformation($"✅ Database and login dropped successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Failed to drop database: {databaseName}");
            return false;
        }
    }

    #region Private Helper Methods

    private async Task<string> GenerateSecurePassword()
    {
        const string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@$?_-";
        var random = new Random();
        var password = new char[16];
        for (int i = 0; i < 16; i++)
        {
            password[i] = validChars[random.Next(validChars.Length)];
        }

        return await Task.FromResult(new string(password));
    }

    private string GetMasterConnectionString()
    {
        var builder = new SqlConnectionStringBuilder(_masterConnectionString)
        {
            InitialCatalog = "master"
        };
        return builder.ConnectionString;
    }

    private async Task<bool> DatabaseExistsAsync(SqlConnection connection, string dbName)
    {
        var query = "SELECT database_id FROM sys.databases WHERE name = @name";
        await using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@name", dbName);

        var result = await cmd.ExecuteScalarAsync();
        return result != null;
    }

    private async Task<bool> CreateDatabaseAsync(SqlConnection connection, string dbName)
    {
        var query = $"CREATE DATABASE [{dbName}]";
        try
        {
            await using var cmd = new SqlCommand(query, connection);
            await cmd.ExecuteNonQueryAsync();

            // Wait for database to be ready
            await Task.Delay(1000);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create database: {dbName}");
            return false;
        }
    }

    //private async Task<bool> CreateLoginAsync(SqlConnection connection, string user, string password)
    //{
    //    var query = $@"
    //        IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = @user)
    //        BEGIN
    //            CREATE LOGIN [{user}] WITH PASSWORD = @password;
    //        END";

    //    try
    //    {
    //        await using var cmd = new SqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@user", user);
    //        cmd.Parameters.AddWithValue("@password", password);
    //        await cmd.ExecuteNonQueryAsync();

    //        // Wait for login to propagate
    //        await Task.Delay(500);

    //        return true;
    //    }
    //    catch (Exception ex)
    //    {
    //        _logger.LogError(ex, $"Failed to create login: {user}");
    //        return false;
    //    }
    //}

    private async Task<bool> CreateLoginAsync(SqlConnection connection, string user, string password)
    {
        try
        {
            // Step 1: Check if login exists (using parameter - safe)
            var checkQuery = "SELECT COUNT(*) FROM sys.server_principals WHERE name = @user";
            await using (var checkCmd = new SqlCommand(checkQuery, connection))
            {
                checkCmd.Parameters.AddWithValue("@user", user);
                //var exists = (int)await checkCmd.ExecuteScalarAsync() > 0;

                var count = (int?)await checkCmd.ExecuteScalarAsync() ?? 0;
                var exists = count > 0;

                if (exists)
                {
                    _logger.LogInformation($"Login already exists: {user}");
                    return true;
                }
            }

            // Step 2: Validate and escape inputs
            if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("Username and password cannot be empty");
            }

            // Escape single quotes in password (SQL injection protection)
            var escapedPassword = password.Replace("'", "''");

            // Escape closing brackets in username
            var escapedUser = user.Replace("]", "]]");

            // Step 3: Create login (password must be literal, cannot be parameter)
            var createQuery = $@"
            CREATE LOGIN [{escapedUser}] 
            WITH PASSWORD = N'{escapedPassword}', 
            DEFAULT_DATABASE = [master],
            CHECK_POLICY = OFF, 
            CHECK_EXPIRATION = OFF";

            await using (var createCmd = new SqlCommand(createQuery, connection))
            {
                await createCmd.ExecuteNonQueryAsync();
            }

            // Wait for login to propagate in SQL Server
            await Task.Delay(500);

            _logger.LogInformation($"✅ Login created successfully: {user}");
            return true;
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, $"❌ SQL Error creating login '{user}': {sqlEx.Message}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Failed to create login: {user}");
            return false;
        }
    }

    private async Task CreateUserAndAssignRoleAsync(SqlConnection connection, string user)
    {
        var query = $@"
            IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = @user)
            BEGIN
                CREATE USER [{user}] FOR LOGIN [{user}];
                ALTER ROLE db_owner ADD MEMBER [{user}];
            END";

        await using var cmd = new SqlCommand(query, connection);
        cmd.Parameters.AddWithValue("@user", user);
        await cmd.ExecuteNonQueryAsync();

        // Wait for permissions to propagate
        await Task.Delay(500);
    }

    private string BuildTenantConnectionString(string dbName, string user, string password)
    {
        var builder = new SqlConnectionStringBuilder(_masterConnectionString)
        {
            InitialCatalog = dbName,
            UserID = user,
            Password = password,
            IntegratedSecurity = false,
            TrustServerCertificate = true
        };

        return builder.ConnectionString;
    }

    private async Task<(bool Success, string? Error)> VerifyTenantConnectionAsync(
        string connectionString,
        string dbName)
    {
        const int maxRetries = 5;
        const int delayMilliseconds = 1000;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                _logger.LogInformation(
                    $"🔄 Connection verification attempt {attempt}/{maxRetries} for: {dbName}");

                await using var connection = new SqlConnection(connectionString);
                await connection.OpenAsync();

                // Execute a simple query to ensure full access
                await using var cmd = new SqlCommand("SELECT 1", connection);
                var result = await cmd.ExecuteScalarAsync();

                if (result != null)
                {
                    _logger.LogInformation($"✅ Connection verified on attempt {attempt}");
                    return (true, null);
                }
            }
            catch (SqlException ex)
            {
                _logger.LogWarning(
                    $"⚠️ Connection attempt {attempt} failed. Error: {ex.Message} (Number: {ex.Number})");

                if (attempt == maxRetries)
                {
                    return (false, $"Cannot connect after {maxRetries} attempts: {ex.Message}");
                }

                // Wait before retry
                await Task.Delay(delayMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Unexpected error during connection verification");
                return (false, $"Unexpected error: {ex.Message}");
            }
        }

        return (false, "Connection verification failed after all retries");
    }

    private async Task CleanupDatabaseAsync(SqlConnection connection, string dbName)
    {
        try
        {
            var query = $@"
                IF EXISTS (SELECT * FROM sys.databases WHERE name = @dbName)
                BEGIN
                    ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                    DROP DATABASE [{dbName}];
                END";

            await using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@dbName", dbName);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation($"✅ Cleaned up database: {dbName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to cleanup database: {dbName}");
        }
    }

    private async Task CleanupLoginAsync(SqlConnection connection, string loginName)
    {
        try
        {
            var query = $@"
                IF EXISTS (SELECT * FROM sys.server_principals WHERE name = @loginName)
                BEGIN
                    DROP LOGIN [{loginName}];
                END";

            await using var cmd = new SqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@loginName", loginName);
            await cmd.ExecuteNonQueryAsync();

            _logger.LogInformation($"✅ Cleaned up login: {loginName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to cleanup login: {loginName}");
        }
    }

    #endregion
}
