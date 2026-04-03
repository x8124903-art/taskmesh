using MsProjects.Application.Models;

namespace MsProjects.Domain.Services;

/// <summary>
/// Service interface for project invitation business logic
/// </summary>
public interface IProjectInvitationService
{
    /// <summary>
    /// Creates a new invitation to join a project
    /// </summary>
    Task<ProjectInvitationModel> CreateInvitationAsync(
        InviteMemberRequest request,
        int invitedByUserId,
        CancellationToken cancellationToken = default,
        string? invitedByName = null);
    
    /// <summary>
    /// Gets invitation by ID
    /// </summary>
    Task<ProjectInvitationModel?> GetInvitationByIdAsync(
        int invitationId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets invitation by token (for accepting/rejecting)
    /// </summary>
    Task<ProjectInvitationModel?> GetInvitationByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all pending invitations for a project
    /// </summary>
    Task<IEnumerable<ProjectInvitationModel>> GetPendingInvitationsAsync(
        int projectId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all invitations for a specific email
    /// </summary>
    Task<IEnumerable<ProjectInvitationModel>> GetInvitationsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Accepts an invitation and adds the user as a member
    /// </summary>
    Task AcceptInvitationAsync(
        string token,
        int userId,
        CancellationToken cancellationToken = default,
        string? userName = null,
        string? email = null);
    
    /// <summary>
    /// Rejects an invitation
    /// </summary>
    Task RejectInvitationAsync(
        string token,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes/cancels an invitation (only by invitation creator or project owner/admin)
    /// </summary>
    Task DeleteInvitationAsync(
        string token,
        CancellationToken cancellationToken = default);
}
