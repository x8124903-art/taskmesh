using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

/// <summary>
/// Request to invite a member to a project
/// </summary>
[ExcludeFromCodeCoverage]
public class InviteMemberRequest
{
    public int ProjectId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
