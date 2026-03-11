using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Domain.Services.Authorization;

namespace MsProjects.Application.UseCases.Project;

public sealed class GetUserProjectsUseCase : IGetUserProjectsUseCase
{
    private readonly IProjectService _projectService;
    private readonly IProjectAuthorizationService _authService;

    public GetUserProjectsUseCase(IProjectService projectService, IProjectAuthorizationService authService)
    {
        _projectService = projectService;
        _authService = authService;
    }

    public async Task<IEnumerable<ProjectModel>> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userProjectIds = await _authService.GetUserProjectIdsAsync(userId, cancellationToken);
        var allProjects = await _projectService.GetAllAsync(cancellationToken);
        return allProjects.Where(p => userProjectIds.Contains(p.IdProject)).ToList();
    }
}
