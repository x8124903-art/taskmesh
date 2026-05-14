using MassTransit;

namespace MsTasks.Infrastructure.EventBus;

public sealed class RabbitMqEventBus(IPublishEndpoint publishEndpoint, ILogger<RabbitMqEventBus> logger) : IEventBus
{
    public async Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            await publishEndpoint.Publish(@event, cancellationToken);
            logger.LogInformation("Published event {EventType} to RabbitMQ", typeof(T).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish event {EventType} to RabbitMQ", typeof(T).Name);
            throw;
        }
    }
}
