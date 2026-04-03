using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.Project;

public sealed class UpdateProjectUseCase : IUpdateProjectUseCase
{
    private readonly IProjectService _projectService;
    private readonly IProjectAuthorizationService _authService;

    public UpdateProjectUseCase(IProjectService projectService, IProjectAuthorizationService authService)
    {
        _projectService = projectService;
        _authService = authService;
    }

    public async Task ExecuteAsync(int projectId, UpdateProjectRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetAsync(projectId, cancellationToken);
        if (project == null)
            throw new KeyNotFoundException($"Project {projectId} not found.");

        if (!await _authService.HasProjectRoleAsync(userId, projectId, cancellationToken,
            ProjectRoles.Owner, ProjectRoles.Admin))
            throw new UnauthorizedAccessException("User does not have permission to update this project.");

        await _projectService.UpdateAsync(projectId, request, cancellationToken);
    }
}
