//using EduSystem.Shared.Event;
//using EduSystem.Shared.Infrastructure.Security;
//using MassTransit;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace EduSystem.Shared.Messaging.Handlers;

//public abstract class BaseMigrationHandler<TDbContext> : IConsumer<TenantDatabaseCreatedEvent>
//    where TDbContext : DbContext
//{
//    private readonly IConnectionStringEncryptor _encryptor;
//    private readonly IEventBus _eventBus;
//    private readonly ILogger _logger;
//    private const int MaxRetries = 3;

//    protected abstract string ServiceName { get; }

//    protected BaseMigrationHandler(
//        IConnectionStringEncryptor encryptor,
//        IEventBus eventBus,
//        ILogger logger)
//    {
//        _encryptor = encryptor;
//        _eventBus = eventBus;
//        _logger = logger;
//    }

//    public async Task Consume(ConsumeContext<TenantDatabaseCreatedEvent> context)
//    {
//        var @event = context.Message;
//        var retryCount = 0;
//        Exception? lastException = null;

//        while (retryCount < MaxRetries)
//        {
//            try
//            {
//                _logger.LogInformation(
//                    $"[{ServiceName}] Applying migrations for tenant: {@event.TenantSlug} (Attempt {retryCount + 1}/{MaxRetries})");

//                if (!_encryptor.Decrypt(@event.EncryptedConnectionString, out var connectionString))
//                {
//                    throw new InvalidOperationException("Failed to decrypt connection string");
//                }

//                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
//                optionsBuilder.UseSqlServer(connectionString);

//                await using var dbContext = CreateDbContext(optionsBuilder.Options);

//                await dbContext.Database.MigrateAsync(context.CancellationToken);

//                _logger.LogInformation(
//                    $"[{ServiceName}] Migrations applied successfully for tenant: {@event.TenantSlug}");

//                await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//                {
//                    TenantId = @event.TenantId,
//                    TenantSlug = @event.TenantSlug,
//                    ServiceName = ServiceName,
//                    Success = true,
//                    CompletedAt = DateTime.UtcNow
//                }, context.CancellationToken);

//                return;
//            }
//            catch (Exception ex)
//            {
//                lastException = ex;
//                retryCount++;

//                _logger.LogWarning(ex,
//                    $"[{ServiceName}] Failed to apply migrations for tenant: {@event.TenantSlug}. Retry {retryCount}/{MaxRetries}");

//                if (retryCount < MaxRetries)
//                {
//                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), context.CancellationToken);
//                }
//            }
//        }

//        _logger.LogError(lastException,
//            $"[{ServiceName}] Failed to apply migrations for tenant: {@event.TenantSlug} after {MaxRetries} attempts");

//        await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//        {
//            TenantId = @event.TenantId,
//            TenantSlug = @event.TenantSlug,
//            ServiceName = ServiceName,
//            Success = false,
//            ErrorMessage = lastException?.Message!,
//            CompletedAt = DateTime.UtcNow
//        }, context.CancellationToken);
//    }

//    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
//}

//using EduSystem.Shared.Event;
//using EduSystem.Shared.Infrastructure.Security;
//using EduSystem.Shared.Messaging;
//using MassTransit;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//public abstract class BaseMigrationHandler<TDbContext> : IConsumer<TenantDatabaseCreatedEvent>
//    where TDbContext : DbContext
//{
//    private readonly IConnectionStringEncryptor _encryptor;
//    private readonly IEventBus _eventBus;
//    private readonly ILogger _logger;
//    private const int MaxRetries = 3;

//    protected abstract string ServiceName { get; }

//    protected BaseMigrationHandler(
//        IConnectionStringEncryptor encryptor,
//        IEventBus eventBus,
//        ILogger logger)
//    {
//        _encryptor = encryptor;
//        _eventBus = eventBus;
//        _logger = logger;
//    }

//    public async Task Consume(ConsumeContext<TenantDatabaseCreatedEvent> context)
//    {
//        var @event = context.Message;
//        var retryCount = 0;
//        Exception? lastException = null;

//        while (retryCount < MaxRetries)
//        {
//            DbContext? dbContext = null;
//            try
//            {
//                _logger.LogInformation(
//                    $"[{ServiceName}] Applying migrations for tenant: {@event.TenantSlug} (Attempt {retryCount + 1}/{MaxRetries})");

//                if (!_encryptor.Decrypt(@event.EncryptedConnectionString, out var connectionString))
//                {
//                    throw new InvalidOperationException("Failed to decrypt connection string");
//                }

//                var builder = new SqlConnectionStringBuilder(connectionString)
//                {
//                    ConnectTimeout = 60,
//                    MultipleActiveResultSets = true,
//                    TrustServerCertificate = true
//                };

//                _logger.LogInformation($"[{ServiceName}] Connecting to database: {builder.DataSource}/{builder.InitialCatalog}");

//                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
//                optionsBuilder.UseSqlServer(builder.ConnectionString, sqlOptions =>
//                {
//                    sqlOptions.CommandTimeout(300); // 5 minutes for migrations
//                    sqlOptions.EnableRetryOnFailure(
//                        maxRetryCount: 3,
//                        maxRetryDelay: TimeSpan.FromSeconds(10),
//                        errorNumbersToAdd: null);
//                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory");
//                });

//                dbContext = CreateDbContext(optionsBuilder.Options);

//                _logger.LogInformation($"[{ServiceName}] Testing database connection...");
//                var canConnect = await dbContext.Database.CanConnectAsync(context.CancellationToken);

//                if (!canConnect)
//                {
//                    throw new InvalidOperationException($"Cannot connect to database: {builder.InitialCatalog}");
//                }

//                _logger.LogInformation($"[{ServiceName}] Connection successful. Applying migrations...");

//                await dbContext.Database.MigrateAsync(context.CancellationToken);

//                _logger.LogInformation(
//                    $"[{ServiceName}] ✅ Migrations applied successfully for tenant: {@event.TenantSlug}");

//                await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//                {
//                    TenantId = @event.TenantId,
//                    TenantSlug = @event.TenantSlug,
//                    ServiceName = ServiceName,
//                    Success = true,
//                    CompletedAt = DateTime.UtcNow
//                }, context.CancellationToken);

//                return; 
//            }
//            catch (SqlException sqlEx)
//            {
//                lastException = sqlEx;
//                retryCount++;

//                _logger.LogWarning(sqlEx,
//                    $"[{ServiceName}] ❌ SQL Error for tenant {@event.TenantSlug}. " +
//                    $"Error Number: {sqlEx.Number}, State: {sqlEx.State}, " +
//                    $"Retry {retryCount}/{MaxRetries}");

//                if (retryCount < MaxRetries)
//                {
//                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount) * 5);
//                    _logger.LogInformation($"⏳ Waiting {delay.TotalSeconds} seconds before retry...");
//                    await Task.Delay(delay, context.CancellationToken);
//                }
//            }
//            catch (Exception ex)
//            {
//                lastException = ex;
//                retryCount++;

//                _logger.LogWarning(ex,
//                    $"[{ServiceName}] ❌ Failed to apply migrations for tenant: {@event.TenantSlug}. " +
//                    $"Retry {retryCount}/{MaxRetries}. Error: {ex.Message}");

//                if (retryCount < MaxRetries)
//                {
//                    var delay = TimeSpan.FromSeconds(Math.Pow(2, retryCount) * 5);
//                    _logger.LogInformation($"⏳ Waiting {delay.TotalSeconds} seconds before retry...");
//                    await Task.Delay(delay, context.CancellationToken);
//                }
//            }
//            finally
//            {
//                if (dbContext != null)
//                {
//                    await dbContext.DisposeAsync();
//                }
//            }
//        }

//        _logger.LogError(lastException,
//            $"[{ServiceName}] ❌❌ Failed to apply migrations for tenant: {@event.TenantSlug} after {MaxRetries} attempts");

//        await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//        {
//            TenantId = @event.TenantId,
//            TenantSlug = @event.TenantSlug,
//            ServiceName = ServiceName,
//            Success = false,
//            ErrorMessage = lastException?.Message ?? "Unknown error",
//            CompletedAt = DateTime.UtcNow
//        }, context.CancellationToken);
//    }

//    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
//}

//using EduSystem.Shared.Event;
//using EduSystem.Shared.Infrastructure.Security;
//using MassTransit;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.Logging;

//namespace EduSystem.Shared.Messaging.Handlers;

//public abstract class BaseMigrationHandler<TDbContext> : IConsumer<TenantDatabaseCreatedEvent>
//    where TDbContext : DbContext
//{
//    private readonly IConnectionStringEncryptor _encryptor;
//    private readonly IEventBus _eventBus;
//    private readonly ILogger _logger;
//    private const int MaxRetries = 5;
//    private const int InitialDelaySeconds = 3;

//    protected abstract string ServiceName { get; }

//    protected BaseMigrationHandler(
//        IConnectionStringEncryptor encryptor,
//        IEventBus eventBus,
//        ILogger logger)
//    {
//        _encryptor = encryptor;
//        _eventBus = eventBus;
//        _logger = logger;
//    }

//    public async Task Consume(ConsumeContext<TenantDatabaseCreatedEvent> context)
//    {
//        var @event = context.Message;
//        var retryCount = 0;
//        Exception? lastException = null;

//        _logger.LogInformation(
//            $"[{ServiceName}] 📨 Received migration event for tenant: {@event.TenantSlug}");

//        // IMPORTANT: Initial delay to ensure database is fully ready
//        _logger.LogInformation(
//            $"[{ServiceName}] ⏳ Waiting {InitialDelaySeconds} seconds before migration...");

//        try
//        {
//            await Task.Delay(TimeSpan.FromSeconds(InitialDelaySeconds), context.CancellationToken);
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning($"[{ServiceName}] ⚠️ Migration cancelled during initial delay");
//            return; // Exit gracefully on cancellation
//        }

//        while (retryCount < MaxRetries)
//        {
//            // Check if cancellation is requested before starting work
//            if (context.CancellationToken.IsCancellationRequested)
//            {
//                _logger.LogWarning($"[{ServiceName}] ⚠️ Migration cancelled before attempt {retryCount + 1}");
//                return;
//            }

//            try
//            {
//                _logger.LogInformation(
//                    $"[{ServiceName}] 🔄 Applying migrations (Attempt {retryCount + 1}/{MaxRetries})");

//                // Decrypt connection string
//                if (!_encryptor.Decrypt(@event.EncryptedConnectionString, out var connectionString))
//                {
//                    throw new InvalidOperationException("Failed to decrypt connection string");
//                }

//                // 🔍 DEBUG: Log decrypted connection string
//                _logger.LogInformation(
//                    $"[{ServiceName}] 🔍 Decrypted connection: {connectionString}");

//                // Test connection first
//                _logger.LogInformation($"[{ServiceName}] 🔌 Testing database connection...");
//                await TestConnectionAsync(connectionString, context.CancellationToken);

//                // Create DbContext and apply migrations
//                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
//                optionsBuilder.UseSqlServer(connectionString);

//                await using var dbContext = CreateDbContext(optionsBuilder.Options);

//                // Check if database exists and is accessible
//                _logger.LogInformation($"[{ServiceName}] 🔍 Checking database connectivity...");
//                var canConnect = await dbContext.Database.CanConnectAsync(context.CancellationToken);

//                if (!canConnect)
//                {
//                    throw new InvalidOperationException(
//                        $"Cannot connect to database for tenant: {@event.TenantSlug}");
//                }

//                // Apply migrations
//                _logger.LogInformation($"[{ServiceName}] 🚀 Applying EF Core migrations...");
//                await dbContext.Database.MigrateAsync(context.CancellationToken);

//                _logger.LogInformation(
//                    $"[{ServiceName}] ✅ Migrations applied successfully for tenant: {@event.TenantSlug}");

//                // Publish success event
//                await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//                {
//                    TenantId = @event.TenantId,
//                    TenantSlug = @event.TenantSlug,
//                    ServiceName = ServiceName,
//                    Success = true,
//                    CompletedAt = DateTime.UtcNow
//                }, context.CancellationToken);

//                return; // Success - exit method
//            }
//            catch (OperationCanceledException)
//            {
//                _logger.LogWarning(
//                    $"[{ServiceName}] ⚠️ Migration cancelled during execution");
//                return; // Exit gracefully on cancellation
//            }
//            catch (Exception ex)
//            {
//                lastException = ex;
//                retryCount++;

//                _logger.LogWarning(ex,
//                    $"[{ServiceName}] ❌ Migration failed (Attempt {retryCount}/{MaxRetries}). " +
//                    $"Error: {ex.Message}");

//                if (retryCount < MaxRetries)
//                {
//                    // Exponential backoff: 2^retry seconds
//                    var delaySeconds = Math.Pow(2, retryCount);
//                    _logger.LogInformation(
//                        $"[{ServiceName}] ⏳ Waiting {delaySeconds} seconds before retry...");

//                    try
//                    {
//                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), context.CancellationToken);
//                    }
//                    catch (OperationCanceledException)
//                    {
//                        _logger.LogWarning(
//                            $"[{ServiceName}] ⚠️ Migration cancelled during retry delay");

//                        // Still publish failure event before exiting
//                        await PublishFailureEventAsync(@event, lastException, CancellationToken.None);
//                        return;
//                    }
//                }
//            }
//        }

//        // All retries failed
//        _logger.LogError(lastException,
//            $"[{ServiceName}] ❌ FAILED: Could not apply migrations after {MaxRetries} attempts");

//        // Publish failure event (use None token to ensure it goes through)
//        await PublishFailureEventAsync(@event, lastException, CancellationToken.None);
//    }

//    private async Task PublishFailureEventAsync(
//        TenantDatabaseCreatedEvent @event,
//        Exception? exception,
//        CancellationToken cancellationToken)
//    {
//        try
//        {
//            await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
//            {
//                TenantId = @event.TenantId,
//                TenantSlug = @event.TenantSlug,
//                ServiceName = ServiceName,
//                Success = false,
//                ErrorMessage = exception?.Message ?? "Unknown error",
//                CompletedAt = DateTime.UtcNow
//            }, cancellationToken);
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"[{ServiceName}] Failed to publish failure event");
//        }
//    }

//    private async Task TestConnectionAsync(string connectionString, CancellationToken cancellationToken)
//    {
//        try
//        {
//            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
//            optionsBuilder.UseSqlServer(connectionString);

//            await using var testContext = CreateDbContext(optionsBuilder.Options);
//            var canConnect = await testContext.Database.CanConnectAsync(cancellationToken);

//            if (!canConnect)
//            {
//                throw new InvalidOperationException("Database connection test failed");
//            }

//            _logger.LogInformation($"[{ServiceName}] Database connection test successful");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, $"[{ServiceName}] Database connection test failed");
//            throw;
//        }
//    }

//    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
//}
using EduSystem.Shared.Event;
using EduSystem.Shared.Infrastructure.Security;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.Shared.Messaging.Handlers;

public abstract class BaseMigrationHandler<TDbContext> : IConsumer<TenantDatabaseCreatedEvent>
    where TDbContext : DbContext
{
    private readonly IConnectionStringEncryptor _encryptor;
    private readonly IEventBus _eventBus;
    private readonly ILogger _logger;
    private const int MaxRetries = 5;
    private const int InitialDelaySeconds = 3;

    protected abstract string ServiceName { get; }

    protected BaseMigrationHandler(
        IConnectionStringEncryptor encryptor,
        IEventBus eventBus,
        ILogger logger)
    {
        _encryptor = encryptor;
        _eventBus = eventBus;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TenantDatabaseCreatedEvent> context)
    {
        var @event = context.Message;
        var retryCount = 0;
        Exception? lastException = null;

        _logger.LogInformation(
            $"[{ServiceName}] 📨 Received migration event for tenant: {@event.TenantSlug}");

        // IMPORTANT: Initial delay to ensure database is fully ready
        _logger.LogInformation(
            $"[{ServiceName}] ⏳ Waiting {InitialDelaySeconds} seconds before migration...");

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(InitialDelaySeconds), context.CancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning($"[{ServiceName}] ⚠️ Migration cancelled during initial delay");
            return; // Exit gracefully on cancellation
        }

        while (retryCount < MaxRetries)
        {
            // Check if cancellation is requested before starting work
            if (context.CancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning($"[{ServiceName}] ⚠️ Migration cancelled before attempt {retryCount + 1}");
                return;
            }

            try
            {
                _logger.LogInformation(
                    $"[{ServiceName}] 🔄 Applying migrations (Attempt {retryCount + 1}/{MaxRetries})");

                // Decrypt connection string
                if (!_encryptor.Decrypt(@event.EncryptedConnectionString, out var connectionString))
                {
                    throw new InvalidOperationException("Failed to decrypt connection string");
                }

                // 🔍 DEBUG: Log decrypted connection string
                _logger.LogInformation(
                    $"[{ServiceName}] 🔍 Decrypted connection: {connectionString}");

                // Test connection first
                _logger.LogInformation($"[{ServiceName}] 🔌 Testing database connection...");
                await TestConnectionAsync(connectionString, context.CancellationToken);

                // Create DbContext and apply migrations
                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                await using var dbContext = CreateDbContext(optionsBuilder.Options);

                // Check if database exists and is accessible
                _logger.LogInformation($"[{ServiceName}] 🔍 Checking database connectivity...");
                var canConnect = await dbContext.Database.CanConnectAsync(context.CancellationToken);

                if (!canConnect)
                {
                    throw new InvalidOperationException(
                        $"Cannot connect to database for tenant: {@event.TenantSlug}");
                }

                // Apply migrations
                _logger.LogInformation($"[{ServiceName}] 🚀 Applying EF Core migrations...");
                await dbContext.Database.MigrateAsync(context.CancellationToken);

                _logger.LogInformation(
                    $"[{ServiceName}] ✅ Migrations applied successfully for tenant: {@event.TenantSlug}");

                // Publish success event
                await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
                {
                    TenantId = @event.TenantId,
                    TenantSlug = @event.TenantSlug,
                    ServiceName = ServiceName,
                    Success = true,
                    CompletedAt = DateTime.UtcNow
                }, context.CancellationToken);

                return; // Success - exit method
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning(
                    $"[{ServiceName}] ⚠️ Migration cancelled during execution");
                return; // Exit gracefully on cancellation
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;

                _logger.LogWarning(ex,
                    $"[{ServiceName}] ❌ Migration failed (Attempt {retryCount}/{MaxRetries}). " +
                    $"Error: {ex.Message}");

                if (retryCount < MaxRetries)
                {
                    // Exponential backoff: 2^retry seconds
                    var delaySeconds = Math.Pow(2, retryCount);
                    _logger.LogInformation(
                        $"[{ServiceName}] ⏳ Waiting {delaySeconds} seconds before retry...");

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), context.CancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning(
                            $"[{ServiceName}] ⚠️ Migration cancelled during retry delay");

                        // Still publish failure event before exiting
                        await PublishFailureEventAsync(@event, lastException, CancellationToken.None);
                        return;
                    }
                }
            }
        }

        // All retries failed
        _logger.LogError(lastException,
            $"[{ServiceName}] ❌ FAILED: Could not apply migrations after {MaxRetries} attempts");

        // Publish failure event (use None token to ensure it goes through)
        await PublishFailureEventAsync(@event, lastException, CancellationToken.None);
    }

    private async Task PublishFailureEventAsync(
        TenantDatabaseCreatedEvent @event,
        Exception? exception,
        CancellationToken cancellationToken)
    {
        try
        {
            await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
            {
                TenantId = @event.TenantId,
                TenantSlug = @event.TenantSlug,
                ServiceName = ServiceName,
                Success = false,
                ErrorMessage = exception?.Message ?? "Unknown error",
                CompletedAt = DateTime.UtcNow
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{ServiceName}] Failed to publish failure event");
        }
    }

    private async Task TestConnectionAsync(string connectionString, CancellationToken cancellationToken)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            await using var testContext = CreateDbContext(optionsBuilder.Options);
            var canConnect = await testContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                throw new InvalidOperationException("Database connection test failed");
            }

            _logger.LogInformation($"[{ServiceName}] Database connection test successful");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[{ServiceName}] Database connection test failed");
            throw;
        }
    }

    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
}
