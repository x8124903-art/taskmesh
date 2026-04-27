namespace MsProjects.Domain.Events;

public sealed record MemberRemovedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    int RemovedUserId,
    int RemovedByUserId
);
