using System.Net;
using System.Net.Http.Json;

namespace MsProjects.Infrastructure.HttpClients;

public sealed class AuthHttpClient(HttpClient httpClient, ILogger<AuthHttpClient> logger) : IAuthHttpClient
{
    public async Task<int?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await httpClient.GetAsync(
                $"users/by-email?email={Uri.EscapeDataString(email)}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
                return null;

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning("Failed to get user by email from ms-auth. Status: {Status}", response.StatusCode);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<UserIdResponse>(cancellationToken: cancellationToken);
            return result?.IdUser;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error calling ms-auth to resolve email '{Email}'", email);
            return null;
        }
    }

    private sealed record UserIdResponse(int IdUser);
}
