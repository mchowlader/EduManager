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
    private const int MaxRetries = 3;

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

        while (retryCount < MaxRetries)
        {
            try
            {
                _logger.LogInformation(
                    $"[{ServiceName}] Applying migrations for tenant: {@event.TenantSlug} (Attempt {retryCount + 1}/{MaxRetries})");

                if (!_encryptor.Decrypt(@event.EncryptedConnectionString, out var connectionString))
                {
                    throw new InvalidOperationException("Failed to decrypt connection string");
                }

                var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
                optionsBuilder.UseSqlServer(connectionString);

                await using var dbContext = CreateDbContext(optionsBuilder.Options);

                await dbContext.Database.MigrateAsync(context.CancellationToken);

                _logger.LogInformation(
                    $"[{ServiceName}] Migrations applied successfully for tenant: {@event.TenantSlug}");

                await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
                {
                    TenantId = @event.TenantId,
                    TenantSlug = @event.TenantSlug,
                    ServiceName = ServiceName,
                    Success = true,
                    CompletedAt = DateTime.UtcNow
                }, context.CancellationToken);

                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;

                _logger.LogWarning(ex,
                    $"[{ServiceName}] Failed to apply migrations for tenant: {@event.TenantSlug}. Retry {retryCount}/{MaxRetries}");

                if (retryCount < MaxRetries)
                {
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), context.CancellationToken);
                }
            }
        }

        _logger.LogError(lastException,
            $"[{ServiceName}] Failed to apply migrations for tenant: {@event.TenantSlug} after {MaxRetries} attempts");

        await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
        {
            TenantId = @event.TenantId,
            TenantSlug = @event.TenantSlug,
            ServiceName = ServiceName,
            Success = false,
            ErrorMessage = lastException?.Message!,
            CompletedAt = DateTime.UtcNow
        }, context.CancellationToken);
    }

    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
}