using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

[ExcludeFromCodeCoverage]
public class ProjectMember
{
    public int IdProjectMember { get; set; }
    public int IdProject { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime? JoinedAt { get; set; }
}
