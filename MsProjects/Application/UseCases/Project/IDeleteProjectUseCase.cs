namespace MsProjects.Application.UseCases.Project;

public interface IDeleteProjectUseCase
{
    Task ExecuteAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
