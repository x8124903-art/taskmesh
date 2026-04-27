namespace MsNotifications.Infrastructure.Persistence;

internal sealed record ProcessedEventEntity(
    string EventId,
    string EventType,
    DateTime ProcessedAt
);
