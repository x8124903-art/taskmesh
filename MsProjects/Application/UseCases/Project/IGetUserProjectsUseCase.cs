using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.Project;

public interface IGetUserProjectsUseCase
{
    Task<IEnumerable<ProjectModel>> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
