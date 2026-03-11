using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.Project;

public sealed class GetProjectDetailsUseCase : IGetProjectDetailsUseCase
{
    private readonly IProjectService _projectService;
    private readonly IProjectAuthorizationService _authService;

    public GetProjectDetailsUseCase(IProjectService projectService, IProjectAuthorizationService authService)
    {
        _projectService = projectService;
        _authService = authService;
    }

    public async Task<ProjectModel?> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        var project = await _projectService.GetAsync(projectId, cancellationToken);
        if (project == null)
            return null;

        if (!await _authService.IsProjectMemberAsync(userId, projectId, cancellationToken))
            throw new UnauthorizedAccessException("User is not a member of this project.");

        return project;
    }
}
