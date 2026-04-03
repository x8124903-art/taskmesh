using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectMember;

public sealed class ChangeProjectMemberRoleUseCase : IChangeProjectMemberRoleUseCase
{
    private readonly IProjectMemberService _memberService;
    private readonly IProjectAuthorizationService _authService;

    public ChangeProjectMemberRoleUseCase(IProjectMemberService memberService, IProjectAuthorizationService authService)
    {
        _memberService = memberService;
        _authService = authService;
    }

    public async Task ExecuteAsync(int projectId, int targetUserId, string role, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await _authService.HasProjectRoleAsync(currentUserId, projectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to change roles in this project.");

        await _memberService.ChangeRoleAsync(projectId, targetUserId, role, cancellationToken);
    }
}
