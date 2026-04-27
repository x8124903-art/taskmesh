namespace MsProjects.Domain.Events;

public sealed record MemberJoinedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    int UserId,
    string RoleName
);
