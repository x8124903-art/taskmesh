namespace MsTasks.Domain.Events;

public sealed record TaskStatusChangedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    TaskStatus OldStatus,
    TaskStatus NewStatus,
    int ChangedByUserId
);
