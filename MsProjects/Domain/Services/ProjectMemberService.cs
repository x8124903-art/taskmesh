using MsProjects.Application.Models;
using MsProjects.Domain.Events;
using MsProjects.Domain.Services.Authorization;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Infrastructure.Repositories;

namespace MsProjects.Domain.Services;

public sealed class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _repository;
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectAuthorizationService _authService;
    private readonly IEventBus _eventBus;
    private readonly ILogger<ProjectMemberService> _logger;

    public ProjectMemberService(
        IProjectMemberRepository repository,
        IProjectRepository projectRepository,
        IProjectAuthorizationService authService,
        IEventBus eventBus,
        ILogger<ProjectMemberService> logger)
    {
        _repository = repository;
        _projectRepository = projectRepository;
        _authService = authService;
        _eventBus = eventBus;
        _logger = logger;
    }

    public Task<IEnumerable<ProjectMemberModel>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default)
        => _repository.GetMembersAsync(projectId, cancellationToken);

    public async Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default)
    {
        await _repository.ChangeRoleAsync(projectId, userId, role, cancellationToken);
        await _authService.InvalidateUserProjectRoleCacheAsync(userId, projectId, cancellationToken);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, int removedByUserId, CancellationToken cancellationToken = default)
    {
        var project = await _projectRepository.GetAsync(projectId, cancellationToken);

        await _repository.RemoveMemberAsync(projectId, userId, cancellationToken);
        await _authService.InvalidateUserProjectsCacheAsync(userId, cancellationToken);

        try
        {
            var evt = new MemberRemovedEvent(
                EventId: Guid.NewGuid().ToString(),
                OccurredAt: DateTime.UtcNow,
                ProjectId: projectId,
                ProjectName: project?.Name ?? "Unknown",
                RemovedUserId: userId,
                RemovedByUserId: removedByUserId
            );
            await _eventBus.PublishAsync(evt, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish MemberRemovedEvent for project {ProjectId}", projectId);
        }
    }
}
