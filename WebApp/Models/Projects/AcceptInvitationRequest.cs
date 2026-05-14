using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

/// <summary>
/// Request to accept an invitation
/// </summary>
[ExcludeFromCodeCoverage]
public class AcceptInvitationRequest
{
    public string Token { get; set; } = string.Empty;
}
