namespace MsProjects.Application.Models;

public sealed record ProjectMemberModel(
    int IdProjectMember, 
    int ProjectId, 
    int UserId, 
    string UserName,
    string Email,
    int Role,
    string RoleName,
    DateTime InvitedAt, 
    DateTime? JoinedAt);
