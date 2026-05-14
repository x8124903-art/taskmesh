namespace MsProjects.Domain.Events;

public sealed record MemberInvitedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    string InvitedEmail,
    string RoleName,
    int InvitedByUserId,
    int? InvitedUserId = null
);
