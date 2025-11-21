namespace EduSystem.Shared.Messaging;

public interface IEventBus
{
    Task PublishAsync<T>(T @Event, CancellationToken cancellationToken = default) where T : class;
}