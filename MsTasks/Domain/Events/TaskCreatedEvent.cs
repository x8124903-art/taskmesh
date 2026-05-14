using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Domain.Events;

[ExcludeFromCodeCoverage]
public sealed record TaskCreatedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    int CreatedBy,
    string TaskTitle
);
