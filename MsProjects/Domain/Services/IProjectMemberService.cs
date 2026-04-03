using MsProjects.Application.Models;

namespace MsProjects.Domain.Services;

public interface IProjectMemberService
{
    Task<IEnumerable<ProjectMemberModel>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default);
    Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
