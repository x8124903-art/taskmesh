using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

/// <summary>
/// Request to reject an invitation
/// </summary>
[ExcludeFromCodeCoverage]
public class RejectInvitationRequest
{
    public string Token { get; set; } = string.Empty;
}
