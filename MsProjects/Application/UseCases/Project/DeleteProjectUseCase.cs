using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.Project;

public sealed class DeleteProjectUseCase : IDeleteProjectUseCase
{
    private readonly IProjectService _projectService;
    private readonly IProjectAuthorizationService _authService;

    public DeleteProjectUseCase(IProjectService projectService, IProjectAuthorizationService authService)
    {
        _projectService = projectService;
        _authService = authService;
    }

    public async Task ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetAsync(projectId, cancellationToken);
        if (project == null)
            throw new KeyNotFoundException($"Project {projectId} not found.");

        if (!await _authService.IsProjectOwnerAsync(userId, projectId, cancellationToken))
            throw new UnauthorizedAccessException("Only the project owner can delete this project.");

        await _projectService.DeleteAsync(projectId, cancellationToken);
    }
}
