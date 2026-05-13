using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Domain.Events;

[ExcludeFromCodeCoverage]
public sealed record TaskAssignedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    int AssignedToUserId,
    int AssignedByUserId,
    string TaskTitle
);
