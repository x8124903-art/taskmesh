using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Infrastructure.Persistence;

[ExcludeFromCodeCoverage]
internal sealed record ProcessedEventEntity(
    string EventId,
    string EventType,
    DateTime ProcessedAt
);
