namespace WebApp.Models.Projects;

/// <summary>
/// Request to reject an invitation
/// </summary>
public class RejectInvitationRequest
{
    public string Token { get; set; } = string.Empty;
}
