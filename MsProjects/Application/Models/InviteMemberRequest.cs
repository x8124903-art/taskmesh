namespace MsProjects.Application.Models;

/// <summary>
/// Request to invite a member to a project via email
/// </summary>
public sealed record InviteMemberRequest(
    int ProjectId, 
    string Email,
    string Role);
