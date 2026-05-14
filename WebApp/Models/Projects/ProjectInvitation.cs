using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

/// <summary>
/// Model for project invitation
/// </summary>
[ExcludeFromCodeCoverage]
public class ProjectInvitation
{
    public int IdProjectInvitation { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int InvitedByUserId { get; set; }
    public string InvitedByName { get; set; } = string.Empty;
    public DateTime InvitedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? RejectedAt { get; set; }
}
