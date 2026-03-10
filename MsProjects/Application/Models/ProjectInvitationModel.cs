namespace MsProjects.Application.Models;

/// <summary>
/// Model for project invitation responses
/// </summary>
public sealed record ProjectInvitationModel(
    int IdProjectInvitation,
    int ProjectId,
    string ProjectName,
    string Email,
    int Role,
    string RoleName,
    string Token,
    string Status,
    int InvitedBy,
    string InvitedByName,
    DateTime CreatedAt,
    DateTime ExpiresAt,
    DateTime? AcceptedAt,
    DateTime? RejectedAt);
