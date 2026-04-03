using MsProjects.Application.Models;
using MsProjects.Domain.Services;

namespace MsProjects.Application.UseCases.Project;

public sealed class CreateProjectUseCase : ICreateProjectUseCase
{
    private readonly IProjectService _projectService;

    public CreateProjectUseCase(IProjectService projectService)
    {
        _projectService = projectService;
    }

    public async Task<ProjectModel> ExecuteAsync(AddProjectRequest request, int userId, string? userName = null, string? userEmail = null, CancellationToken cancellationToken = default)
    {
        return await _projectService.AddAsync(request, userId, cancellationToken, userName, userEmail);
    }
}
