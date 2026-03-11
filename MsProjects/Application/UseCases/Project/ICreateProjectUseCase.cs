using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.Project;

public interface ICreateProjectUseCase
{
    Task<ProjectModel> ExecuteAsync(AddProjectRequest request, int userId, string? userName = null, string? userEmail = null, CancellationToken cancellationToken = default);
}
