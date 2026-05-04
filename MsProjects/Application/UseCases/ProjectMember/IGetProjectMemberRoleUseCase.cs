using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectMember;

public interface IGetProjectMemberRoleUseCase
{
    Task<MemberRoleResponse?> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
