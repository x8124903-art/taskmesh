using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.ProjectMember;

public interface IGetProjectMembersUseCase
{
    Task<IEnumerable<ProjectMemberModel>> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
