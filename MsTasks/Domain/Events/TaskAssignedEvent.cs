namespace MsTasks.Domain.Events;

public sealed record TaskAssignedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    int AssignedToUserId,
    int AssignedByUserId,
    string TaskTitle
);
