using MsProjects.Application.Models;

namespace MsProjects.Infrastructure.Repositories;

/// <summary>
/// Repository interface for project invitation operations
/// </summary>
public interface IProjectInvitationRepository
{
    /// <summary>
    /// Creates a new project invitation
    /// </summary>
    Task<ProjectInvitationModel> CreateAsync(
        int projectId, 
        string email, 
        string role, 
        string token, 
        int invitedByUserId,
        string? invitedByName,
        DateTime expiresAt,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets invitation by ID
    /// </summary>
    Task<ProjectInvitationModel?> GetByIdAsync(
        int invitationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets invitation by token
    /// </summary>
    Task<ProjectInvitationModel?> GetByTokenAsync(
        string token, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all pending invitations for a project
    /// </summary>
    Task<IEnumerable<ProjectInvitationModel>> GetPendingByProjectIdAsync(
        int projectId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all invitations for a specific email
    /// </summary>
    Task<IEnumerable<ProjectInvitationModel>> GetByEmailAsync(
        string email, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Accepts an invitation (updates status to Accepted and sets AcceptedAt)
    /// </summary>
    Task AcceptAsync(
        int invitationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Rejects an invitation (updates status to Rejected and sets RejectedAt)
    /// </summary>
    Task RejectAsync(
        int invitationId, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes an invitation permanently
    /// </summary>
    Task DeleteAsync(
        int invitationId,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Marks expired invitations (updates status to Expired for invitations past ExpiresAt)
    /// </summary>
    Task MarkExpiredInvitationsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an invitation exists for email in project
    /// </summary>
    Task<bool> ExistsPendingByProjectAndEmailAsync(
        int projectId, 
        string email, 
        CancellationToken cancellationToken = default);
}
