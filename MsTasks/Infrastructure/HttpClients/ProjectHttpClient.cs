using System.Net;
using System.Net.Http.Json;

namespace MsTasks.Infrastructure.HttpClients;

public sealed class ProjectHttpClient : IProjectHttpClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectHttpClient> _logger;

    public ProjectHttpClient(HttpClient httpClient, ILogger<ProjectHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<string?> GetUserRoleInProjectAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"projects/{projectId}/members/{userId}/role",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to get user role. Status: {Status}", response.StatusCode);
                throw new HttpRequestException($"Failed to get user role from ms-projects. Status: {response.StatusCode}");
            }

            var result = await response.Content.ReadFromJsonAsync<MemberRoleResponse>(cancellationToken: cancellationToken);
            
            return result?.Role;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling ms-projects for user {UserId} in project {ProjectId}", userId, projectId);
            throw new HttpRequestException("ms-projects service unavailable", ex);
        }
    }
}
