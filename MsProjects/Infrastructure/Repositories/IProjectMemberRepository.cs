using MsProjects.Application.Models;

namespace MsProjects.Infrastructure.Repositories;

public interface IProjectMemberRepository
{
    Task<IEnumerable<ProjectMemberModel>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default);
    Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default);
    
    Task<string?> GetUserRoleInProjectAsync(int userId, int projectId, CancellationToken cancellationToken = default);
    Task<List<int>> GetProjectIdsByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByProjectIdAndUserIdAsync(int projectId, int userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsMemberByProjectIdAndEmailAsync(int projectId, string email, CancellationToken cancellationToken = default);
    Task<int?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task AddMemberAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default, string? userName = null, string? email = null);
}
