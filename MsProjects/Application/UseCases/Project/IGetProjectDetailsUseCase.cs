using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.Project;

public interface IGetProjectDetailsUseCase
{
    Task<ProjectModel?> ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
