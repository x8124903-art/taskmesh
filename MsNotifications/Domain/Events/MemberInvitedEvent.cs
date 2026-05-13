using System.Diagnostics.CodeAnalysis;

namespace MsProjects.Domain.Events;

[ExcludeFromCodeCoverage]
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
