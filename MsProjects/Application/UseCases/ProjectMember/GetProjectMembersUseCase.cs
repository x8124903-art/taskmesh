using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectMember;

public sealed class GetProjectMembersUseCase : IGetProjectMembersUseCase
{
    private readonly IProjectMemberService _memberService;
    private readonly IProjectAuthorizationService _authService;

    public GetProjectMembersUseCase(IProjectMemberService memberService, IProjectAuthorizationService authService)
    {
        _memberService = memberService;
        _authService = authService;
    }

    public async Task<IEnumerable<ProjectMemberModel>> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        if (!await _authService.IsProjectMemberAsync(userId, projectId, cancellationToken))
            throw new UnauthorizedAccessException("User is not a member of this project.");

        return await _memberService.GetMembersAsync(projectId, cancellationToken);
    }
}
