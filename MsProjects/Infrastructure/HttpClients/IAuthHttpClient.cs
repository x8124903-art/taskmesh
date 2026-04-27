namespace MsProjects.Infrastructure.HttpClients;

public interface IAuthHttpClient
{
    Task<int?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);
}
