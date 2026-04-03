using MsProjects.Application.Models;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.Repositories;

namespace MsProjects.Domain.Services;

public sealed class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _repository;
    private readonly IProjectAuthorizationService _authService;

    public ProjectMemberService(
        IProjectMemberRepository repository,
        IProjectAuthorizationService authService)
    {
        _repository = repository;
        _authService = authService;
    }

    public Task<IEnumerable<ProjectMemberModel>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default)
        => _repository.GetMembersAsync(projectId, cancellationToken);

    public async Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default)
    {
        await _repository.ChangeRoleAsync(projectId, userId, role, cancellationToken);
        await _authService.InvalidateUserProjectRoleCacheAsync(userId, projectId, cancellationToken);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        await _repository.RemoveMemberAsync(projectId, userId, cancellationToken);
        await _authService.InvalidateUserProjectsCacheAsync(userId, cancellationToken);
    }
}
