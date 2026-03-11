using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectMember;

public sealed class RemoveProjectMemberUseCase : IRemoveProjectMemberUseCase
{
    private readonly IProjectMemberService _memberService;
    private readonly IProjectAuthorizationService _authService;

    public RemoveProjectMemberUseCase(IProjectMemberService memberService, IProjectAuthorizationService authService)
    {
        _memberService = memberService;
        _authService = authService;
    }

    public async Task ExecuteAsync(int projectId, int targetUserId, int currentUserId, CancellationToken cancellationToken = default)
    {
        if (!await _authService.HasProjectRoleAsync(currentUserId, projectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to remove members from this project.");

        await _memberService.RemoveMemberAsync(projectId, targetUserId, cancellationToken);
    }
}
