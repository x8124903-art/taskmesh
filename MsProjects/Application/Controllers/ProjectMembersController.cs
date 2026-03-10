using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.Controllers;

[ApiController]
[Route("projects/{projectId}/members")]
public sealed class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _service;
    private readonly IProjectAuthorizationService _authService;

    public ProjectMembersController(
        IProjectMemberService service,
        IProjectAuthorizationService authService)
    {
        _service = service;
        _authService = authService;
    }

    private int GetCurrentUserId()
    {
        var userIdHeader = Request.Headers["X-User-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(userIdHeader))
            throw new UnauthorizedAccessException("User authentication required. X-User-Id header not found.");
        
        if (!int.TryParse(userIdHeader, out var userId))
            throw new ArgumentException($"Invalid X-User-Id header value: {userIdHeader}");
        
        return userId;
    }

    [HttpGet]
    public async Task<IActionResult> GetMembers(int projectId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        if (!await _authService.IsProjectMemberAsync(currentUserId, projectId, cancellationToken))
        {
            return Forbid();
        }
        
        var members = await _service.GetMembersAsync(projectId, cancellationToken);
        return Ok(members);
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> ChangeRole(int projectId, int userId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        if (!await _authService.HasProjectRoleAsync(currentUserId, projectId, cancellationToken, 
            ProjectRoles.Owner, ProjectRoles.Admin))
        {
            return Forbid();
        }
        
        await _service.ChangeRoleAsync(projectId, userId, request.Role, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(int projectId, int userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        
        if (!await _authService.HasProjectRoleAsync(currentUserId, projectId, cancellationToken, 
            ProjectRoles.Owner, ProjectRoles.Admin))
        {
            return Forbid();
        }
        
        await _service.RemoveMemberAsync(projectId, userId, cancellationToken);
        return NoContent();
    }
}
