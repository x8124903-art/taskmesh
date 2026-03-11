using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectInvitation;

namespace MsProjects.Application.Controllers;

[ApiController]
[Route("invitations")]
public sealed class ProjectInvitationsController : ControllerBase
{
    private readonly ICreateProjectInvitationUseCase _createInvitation;
    private readonly IGetInvitationDetailsUseCase _getInvitationDetails;
    private readonly IGetInvitationByTokenUseCase _getInvitationByToken;
    private readonly IGetProjectPendingInvitationsUseCase _getPendingInvitations;
    private readonly IGetMyInvitationsUseCase _getMyInvitations;
    private readonly IAcceptProjectInvitationUseCase _acceptInvitation;
    private readonly IRejectProjectInvitationUseCase _rejectInvitation;
    private readonly ICancelProjectInvitationUseCase _cancelInvitation;

    public ProjectInvitationsController(
        ICreateProjectInvitationUseCase createInvitation,
        IGetInvitationDetailsUseCase getInvitationDetails,
        IGetInvitationByTokenUseCase getInvitationByToken,
        IGetProjectPendingInvitationsUseCase getPendingInvitations,
        IGetMyInvitationsUseCase getMyInvitations,
        IAcceptProjectInvitationUseCase acceptInvitation,
        IRejectProjectInvitationUseCase rejectInvitation,
        ICancelProjectInvitationUseCase cancelInvitation)
    {
        _createInvitation = createInvitation;
        _getInvitationDetails = getInvitationDetails;
        _getInvitationByToken = getInvitationByToken;
        _getPendingInvitations = getPendingInvitations;
        _getMyInvitations = getMyInvitations;
        _acceptInvitation = acceptInvitation;
        _rejectInvitation = rejectInvitation;
        _cancelInvitation = cancelInvitation;
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

    [HttpPost]
    public async Task<IActionResult> CreateInvitation(
        [FromBody] InviteMemberRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();

        var invitation = await _createInvitation.ExecuteAsync(request, currentUserId, userName, cancellationToken);
        
        return CreatedAtAction(
            nameof(GetInvitationById), 
            new { id = invitation.IdProjectInvitation }, 
            invitation);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetInvitationById(
        int id,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var invitation = await _getInvitationDetails.ExecuteAsync(id, currentUserId, cancellationToken);
        return Ok(invitation);
    }

    [HttpGet("by-token/{token}")]
    public async Task<IActionResult> GetInvitationByToken(
        string token,
        CancellationToken cancellationToken)
    {
        var invitation = await _getInvitationByToken.ExecuteAsync(token, cancellationToken);
        return Ok(invitation);
    }

    [HttpGet("project/{projectId:int}")]
    public async Task<IActionResult> GetPendingInvitations(
        int projectId,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var invitations = await _getPendingInvitations.ExecuteAsync(projectId, currentUserId, cancellationToken);
        return Ok(invitations);
    }

    [HttpGet("my-invitations")]
    public async Task<IActionResult> GetMyInvitations(
        [FromQuery] string email,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email is required");

        var invitations = await _getMyInvitations.ExecuteAsync(email, status, cancellationToken);
        return Ok(invitations);
    }

    [HttpPost("accept")]
    public async Task<IActionResult> AcceptInvitation(
        [FromBody] AcceptInvitationRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        var userName = Request.Headers["X-User-Name"].FirstOrDefault();
        var userEmail = Request.Headers["X-User-Email"].FirstOrDefault();
        
        await _acceptInvitation.ExecuteAsync(request.Token, currentUserId, userName, userEmail, cancellationToken);
        return Ok(new { Message = "Invitation accepted successfully" });
    }

    [HttpPost("reject")]
    public async Task<IActionResult> RejectInvitation(
        [FromBody] RejectInvitationRequest request,
        CancellationToken cancellationToken)
    {
        await _rejectInvitation.ExecuteAsync(request.Token, cancellationToken);
        return Ok(new { Message = "Invitation rejected successfully" });
    }

    [HttpDelete("{token}")]
    public async Task<IActionResult> DeleteInvitation(
        string token,
        CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _cancelInvitation.ExecuteAsync(token, currentUserId, cancellationToken);
        return NoContent();
    }
}
