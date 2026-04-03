using MsProjects.Application.Models;

namespace MsProjects.Application.UseCases.Project;

public interface IUpdateProjectUseCase
{
    Task ExecuteAsync(int projectId, UpdateProjectRequest request, int userId, CancellationToken cancellationToken = default);
}
