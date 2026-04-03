namespace WebApp.Models.Projects;

/// <summary>
/// Request to accept an invitation
/// </summary>
public class AcceptInvitationRequest
{
    public string Token { get; set; } = string.Empty;
}
