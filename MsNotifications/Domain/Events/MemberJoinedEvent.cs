using System.Diagnostics.CodeAnalysis;

namespace MsProjects.Domain.Events;

[ExcludeFromCodeCoverage]
public sealed record MemberJoinedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    int UserId,
    string RoleName
);
