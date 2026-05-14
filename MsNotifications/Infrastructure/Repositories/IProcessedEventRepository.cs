namespace MsNotifications.Infrastructure.Repositories;

public interface IProcessedEventRepository
{
    Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default);
    Task MarkEventAsProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default);
}
