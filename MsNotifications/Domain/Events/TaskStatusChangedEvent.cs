namespace MsTasks.Domain.Events;

public sealed record TaskStatusChangedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    string OldStatus,
    string NewStatus,
    int ChangedByUserId,
    string TaskTitle,
    int? AssignedToUserId,
    int TaskCreatedByUserId
);
