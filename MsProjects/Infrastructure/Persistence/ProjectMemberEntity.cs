namespace MsProjects.Infrastructure.Persistence;

internal sealed record ProjectMemberEntity(
    int IdProjectMember,
    int ProjectId,
    int UserId,
    string UserName,
    string Email,
    int Role,
    string RoleName,
    DateTime InvitedAt,
    DateTime? JoinedAt);
