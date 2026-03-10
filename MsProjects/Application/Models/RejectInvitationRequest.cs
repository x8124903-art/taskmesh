namespace MsProjects.Application.Models;

/// <summary>
/// Request to reject a project invitation
/// </summary>
public sealed record RejectInvitationRequest(string Token);
