namespace MsTasks.Application.UseCases.Task;

public interface IDeleteTaskUseCase
{
    System.Threading.Tasks.Task ExecuteAsync(int taskId, int currentUserId, CancellationToken cancellationToken = default);
}
