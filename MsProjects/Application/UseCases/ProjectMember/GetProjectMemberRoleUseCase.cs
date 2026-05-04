using MsProjects.Application.Models;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.ProjectMember;

public sealed class GetProjectMemberRoleUseCase(IProjectAuthorizationService authorizationService) 
    : IGetProjectMemberRoleUseCase
{
    public async Task<MemberRoleResponse?> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        var role = await authorizationService.GetUserProjectRoleAsync(userId, projectId, cancellationToken);
        
        return role is not null ? new MemberRoleResponse(role) : null;
    }
}
