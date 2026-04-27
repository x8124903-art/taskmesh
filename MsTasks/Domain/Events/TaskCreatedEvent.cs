namespace MsTasks.Domain.Events;

public sealed record TaskCreatedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    int CreatedBy,
    string TaskTitle
);
