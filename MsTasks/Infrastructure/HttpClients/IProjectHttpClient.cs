namespace MsTasks.Infrastructure.HttpClients;

public interface IProjectHttpClient
{
    Task<string?> GetUserRoleInProjectAsync(int projectId, int userId, CancellationToken cancellationToken = default);
}
