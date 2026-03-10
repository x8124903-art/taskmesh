using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.Controllers;

[ApiController]
[Route("invitations")]
public sealed class ProjectInvitationsController : ControllerBase
{
    private readonly IProjectInvitationService _invitationService;
    private readonly IProjectAuthorizationService _authService;
    private readonly ILogger<ProjectInvitationsController> _logger;

    public ProjectInvitationsController(
        IProjectInvitationService invitationService,
        IProjectAuthorizationService authService,
        ILogger<ProjectInvitationsController> logger)
    {
        _invitationService = invitationService;
        _authService = authService;
        _logger = logger;
    }

    private int GetCurrentUserId()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        
        if (string.IsNullOrEmpty(userIdHeader))
            throw new UnauthorizedAccessException("User authentication required. X-User-Id header not found.");
        
        if (!int.TryParse(userIdHeader, out var userId))
            throw new ArgumentException($"Invalid X-User-Id header value: '{userIdHeader}'");
        
        return userId;
    }

    /// <summary>
    /// Creates a new invitation to join a project (Owner/Admin only)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateInvitation(
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        if (!await _authService.HasProjectRoleAsync(
            currentUserId, 
            request.ProjectId, 
            cancellationToken, 
            ProjectRoles.Owner, 
            ProjectRoles.Admin))
        {
            return StatusCode(403);
        }

        var invitation = await _invitationService.CreateInvitationAsync(
            request, 
            currentUserId, 
            cancellationToken,
            Request.Headers["X-User-Name"].FirstOrDefault());
        
        return CreatedAtAction(
            nameof(GetInvitationById), 
            new { id = invitation.IdProjectInvitation }, 
            invitation);
    }

    /// <summary>
    /// Gets an invitation by ID (Owner/Admin only)
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvitationById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        var invitation = await _invitationService.GetInvitationByIdAsync(id, cancellationToken);
        
        if (invitation == null)
        {
            return NotFound();
        }

        if (!await _authService.HasProjectRoleAsync(
            currentUserId, 
            invitation.ProjectId, 
            cancellationToken, 
            ProjectRoles.Owner, 
            ProjectRoles.Admin))
        {
            return StatusCode(403);
        }

        return Ok(invitation);
    }

    /// <summary>
    /// Gets an invitation by token (public, for accepting/rejecting)
    /// </summary>
    [HttpGet("by-token/{token}")]
    public async Task<IActionResult> GetInvitationByToken(
        string token,
        CancellationToken cancellationToken)
    {
        var invitation = await _invitationService.GetInvitationByTokenAsync(token, cancellationToken);
        
        if (invitation == null)
        {
            return NotFound();
        }

        return Ok(invitation);
    }

    /// <summary>
    /// Gets all pending invitations for a project (Owner/Admin only)
    /// </summary>
    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetPendingInvitations(
        int projectId,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        if (!await _authService.HasProjectRoleAsync(
            currentUserId, 
            projectId, 
            cancellationToken, 
            ProjectRoles.Owner, 
            ProjectRoles.Admin))
        {
            return StatusCode(403);
        }

        var invitations = await _invitationService.GetPendingInvitationsAsync(
            projectId, 
            cancellationToken);
        
        return Ok(invitations);
    }

    /// <summary>
    /// Gets all invitations for the current user's email
    /// Optional status filtering: Pending, Accepted, Rejected
    /// </summary>
    [HttpGet("my-invitations")]
    public async Task<IActionResult> GetMyInvitations(
        [FromQuery] string email,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest("Email is required");
        }

        var invitations = await _invitationService.GetInvitationsByEmailAsync(
            email, 
            cancellationToken);
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            invitations = invitations.Where(i => 
                string.Equals(i.Status, status, StringComparison.OrdinalIgnoreCase));
        }
        
        return Ok(invitations);
    }

    /// <summary>
    /// Accepts an invitation
    /// </summary>
    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();
        var userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
        
        await _invitationService.AcceptInvitationAsync(
            request.Token, 
            currentUserId, 
            cancellationToken,
            userName,
            userEmail);
        
        return Ok(new { Message = "Invitation accepted successfully" });
    }

    /// <summary>
    /// Rejects an invitation
    /// </summary>
    [HttpPost("reject")]
    public async Task<IActionResult> RejectInvitation(
        [FromBody] RejectInvitationRequest request,
        CancellationToken cancellationToken)
    {
        await _invitationService.RejectInvitationAsync(request.Token, cancellationToken);
        
        return Ok(new { Message = "Invitation rejected successfully" });
    }

    /// <summary>
    /// Deletes/cancels an invitation (only by project owner/admin)
    /// </summary>
    [HttpDelete("{token}")]
    public async Task<IActionResult> DeleteInvitation(
        string token,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        var invitation = await _invitationService.GetInvitationByTokenAsync(token, cancellationToken);
        
        if (invitation == null)
        {
            return NotFound(new { Message = "Invitation not found" });
        }

        if (!await _authService.HasProjectRoleAsync(
            currentUserId, 
            invitation.ProjectId, 
            cancellationToken,
            ProjectRoles.Owner, 
            ProjectRoles.Admin))
        {
            return StatusCode(403);
        }

        await _invitationService.DeleteInvitationAsync(token, cancellationToken);
        
        return NoContent();
    }
}
