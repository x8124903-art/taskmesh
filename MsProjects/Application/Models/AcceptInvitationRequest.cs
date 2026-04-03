namespace MsProjects.Application.Models;

/// <summary>
/// Request to accept a project invitation
/// </summary>
public sealed record AcceptInvitationRequest(string Token);
