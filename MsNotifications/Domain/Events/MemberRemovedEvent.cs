using System.Diagnostics.CodeAnalysis;

namespace MsProjects.Domain.Events;

[ExcludeFromCodeCoverage]
public sealed record MemberRemovedEvent(
    string EventId,
    DateTime OccurredAt,
    int ProjectId,
    string ProjectName,
    int RemovedUserId,
    int RemovedByUserId
);
