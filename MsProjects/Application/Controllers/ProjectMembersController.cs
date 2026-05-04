using Microsoft.AspNetCore.Mvc;
using MsProjects.Application.Models;
using MsProjects.Application.UseCases.ProjectMember;

namespace MsProjects.Application.Controllers;

[ApiController]
[Route("projects/{projectId}/members")]
public sealed class ProjectMembersController : ControllerBase
{
    private readonly IGetProjectMembersUseCase _getMembers;
    private readonly IChangeProjectMemberRoleUseCase _changeRole;
    private readonly IRemoveProjectMemberUseCase _removeMember;
    private readonly IGetProjectMemberRoleUseCase _getRole;

    public ProjectMembersController(
        IGetProjectMembersUseCase getMembers,
        IChangeProjectMemberRoleUseCase changeRole,
        IRemoveProjectMemberUseCase removeMember,
        IGetProjectMemberRoleUseCase getRole)
    {
        _getMembers = getMembers;
        _changeRole = changeRole;
        _removeMember = removeMember;
        _getRole = getRole;
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
        var members = await _getMembers.ExecuteAsync(projectId, currentUserId, cancellationToken);
        return Ok(members);
    }

    [HttpGet("{userId}/role")]
    public async Task<IActionResult> GetRole(int projectId, int userId, CancellationToken cancellationToken)
    {
        var result = await _getRole.ExecuteAsync(projectId, userId, cancellationToken);
        
        if (result is null)
            return NotFound(new { message = "User is not a member of this project." });
        
        return Ok(result);
    }

    [HttpPut("{userId}/role")]
    public async Task<IActionResult> ChangeRole(int projectId, int userId, [FromBody] ChangeRoleRequest request, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _changeRole.ExecuteAsync(projectId, userId, request.Role, currentUserId, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{userId}")]
    public async Task<IActionResult> RemoveMember(int projectId, int userId, CancellationToken cancellationToken)
    {
        var currentUserId = GetCurrentUserId();
        await _removeMember.ExecuteAsync(projectId, userId, currentUserId, cancellationToken);
        return NoContent();
    }
}
