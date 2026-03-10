using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using WebApp.Models.Projects;

namespace WebApp.Services;

/// <summary>
/// API service for project invitation operations
/// </summary>
public class ProjectInvitationApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ProjectInvitationApiService> _logger;

    public ProjectInvitationApiService(HttpClient httpClient, ILogger<ProjectInvitationApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    /// <summary>
    /// Gets all invitations for the user's email with optional status filtering
    /// </summary>
    /// <param name="email">User's email address</param>
    /// <param name="status">Optional status filter: Pending, Accepted, Rejected</param>
    public async Task<List<ProjectInvitation>> GetMyInvitationsAsync(string email, string? status = null)
    {
        var url = $"/api/v1/invitations/my-invitations?email={Uri.EscapeDataString(email)}";
        
        if (!string.IsNullOrWhiteSpace(status))
        {
            url += $"&status={Uri.EscapeDataString(status)}";
        }

        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            return new List<ProjectInvitation>();
        }

        return await response.Content.ReadFromJsonAsync<List<ProjectInvitation>>() 
               ?? new List<ProjectInvitation>();
    }

    /// <summary>
    /// Accepts an invitation
    /// </summary>
    public async Task<bool> AcceptInvitationAsync(string token)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/invitations/accept", 
                new { Token = token });
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error accepting invitation with token {Token}", token);
            return false;
        }
    }

    /// <summary>
    /// Rejects an invitation
    /// </summary>
    public async Task<bool> RejectInvitationAsync(string token)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/invitations/reject", 
                new { Token = token });
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting invitation with token {Token}", token);
            return false;
        }
    }

    /// <summary>
    /// Deletes/cancels an invitation (only by project owner/admin)
    /// </summary>
    public async Task<bool> DeleteInvitationAsync(string token)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/invitations/{token}");
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting invitation with token {Token}", token);
            return false;
        }
    }
}
