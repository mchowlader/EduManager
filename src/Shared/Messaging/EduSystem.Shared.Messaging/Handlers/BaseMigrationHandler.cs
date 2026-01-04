using EduSystem.Shared.Event;
using EduSystem.Shared.Infrastructure.MultiTenancy;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EduSystem.Shared.Messaging.Handlers;

public abstract class BaseMigrationHandler<TDbContext>(
    ITenantMigrationService<TDbContext> migrationService,
    IEventBus eventBus,
    ILogger logger)
    : IConsumer<TenantDatabaseCreatedEvent>
    where TDbContext : DbContext
{
    private readonly ITenantMigrationService<TDbContext> _migrationService = migrationService;
    private readonly IEventBus _eventBus = eventBus;
    private readonly ILogger _logger = logger;

    protected abstract string ServiceName { get; }

    public async Task Consume(ConsumeContext<TenantDatabaseCreatedEvent> context)
    {
        var @event = context.Message;

        try
        {
            _logger.LogInformation("[{ServiceName}] 📨 Received migration event for tenant: {TenantSlug}", 
                ServiceName, @event.TenantSlug);

            await _migrationService.MigrateAsync(@event.EncryptedConnectionString, @event.TenantSlug, context.CancellationToken);

            // Publish success event
            await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
            {
                TenantId = @event.TenantId,
                TenantSlug = @event.TenantSlug,
                ServiceName = ServiceName,
                Success = true,
                CompletedAt = DateTime.UtcNow
            }, context.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{ServiceName}] ❌ Migration failed for tenant: {TenantSlug}", 
                ServiceName, @event.TenantSlug);

            // Publish failure event
            await _eventBus.PublishAsync(new ServiceMigrationCompletedEvent
            {
                TenantId = @event.TenantId,
                TenantSlug = @event.TenantSlug,
                ServiceName = ServiceName,
                Success = false,
                ErrorMessage = ex.Message,
                CompletedAt = DateTime.UtcNow
            }, context.CancellationToken);
        }
    }

    protected abstract TDbContext CreateDbContext(DbContextOptions<TDbContext> options);
}
