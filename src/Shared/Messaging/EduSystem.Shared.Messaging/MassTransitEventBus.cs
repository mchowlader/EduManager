using MassTransit;
using Microsoft.Extensions.Logging;

namespace EduSystem.Shared.Messaging;

public class MassTransitEventBus(IPublishEndpoint publishEndpoint, ILogger<MassTransitEventBus> logger) : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly ILogger<MassTransitEventBus> _logger = logger;

    public async Task PublishAsync<T>(T Event, CancellationToken cancellationToken = default) where T : class
    {
		try
		{
			_logger.LogInformation($"Publishing event: {typeof(T).Name}");
			await _publishEndpoint.Publish(Event, cancellationToken)
				.ConfigureAwait(false);
			_logger.LogInformation($"Event published: {typeof(T).Name}");
        }
		catch (Exception ex)
		{
			_logger.LogError(ex.StackTrace, $"Failed to publish event: {typeof(T).Name}");
		}
    }
}
