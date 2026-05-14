namespace MsProjects.Domain.Events;

public sealed record ProjectCreatedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    int OwnerId
);
