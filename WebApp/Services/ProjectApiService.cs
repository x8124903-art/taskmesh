using System.Net.Http.Json;
using WebApp.Models.Projects;

namespace WebApp.Services;

public class ProjectApiService(HttpClient httpClient)
{
    public async Task<List<Project>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<Project>>("/api/v1/projects", cancellationToken)
            ?? new List<Project>();
    }

    public async Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<Project>($"/api/v1/projects/{id}", cancellationToken);
    }

    public async Task<Project?> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/projects", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await response.Content.ReadFromJsonAsync<Project>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(int id, string name, string description, string status, CancellationToken cancellationToken = default)
    {
        var statusId = status switch
        {
            "Active" => 1,
            "Paused" => 2,
            "Completed" => 3,
            "Archived" => 4,
            _ => 1
        };
        
        var request = new { Name = name, Description = description, Status = statusId };
        var response = await httpClient.PutAsJsonAsync($"/api/v1/projects/{id}", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/projects/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ProjectMember>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ProjectMember>>($"/api/v1/projects/{projectId}/members", cancellationToken)
            ?? new List<ProjectMember>();
    }

    public async Task<bool> ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default)
    {
        var request = new { Role = role };
        var response = await httpClient.PutAsJsonAsync($"/api/v1/projects/{projectId}/members/{userId}/role", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/projects/{projectId}/members/{userId}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<ProjectInvitation?> CreateInvitationAsync(InviteMemberRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/invitations", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await response.Content.ReadFromJsonAsync<ProjectInvitation>(cancellationToken: cancellationToken);
    }

    public async Task<ProjectInvitation?> GetInvitationByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<ProjectInvitation>($"/api/v1/invitations/by-token/{token}", cancellationToken);
    }

    public async Task<List<ProjectInvitation>> GetPendingInvitationsAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ProjectInvitation>>($"/api/v1/invitations/project/{projectId}", cancellationToken)
            ?? new List<ProjectInvitation>();
    }

    public async Task<List<ProjectInvitation>> GetMyInvitationsAsync(string email, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<ProjectInvitation>>($"/api/v1/invitations/my-invitations?email={Uri.EscapeDataString(email)}", cancellationToken)
            ?? new List<ProjectInvitation>();
    }

    public async Task<bool> AcceptInvitationAsync(string token, CancellationToken cancellationToken = default)
    {
        var request = new AcceptInvitationRequest { Token = token };
        var response = await httpClient.PostAsJsonAsync("/api/v1/invitations/accept", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectInvitationAsync(string token, CancellationToken cancellationToken = default)
    {
        var request = new RejectInvitationRequest { Token = token };
        var response = await httpClient.PostAsJsonAsync("/api/v1/invitations/reject", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }
}
