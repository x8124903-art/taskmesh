using System.Net.Http.Json;
using WebApp.Models.Tasks;

namespace WebApp.Services;

public class TaskApiService(HttpClient httpClient)
{
    public async Task<List<TaskModel>> GetByProjectAsync(
        int projectId, 
        string? status = null, 
        string? priority = null, 
        int? assignedToUserId = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string> { $"projectId={projectId}" };
        
        if (!string.IsNullOrEmpty(status))
            queryParams.Add($"status={Uri.EscapeDataString(status)}");
        
        if (!string.IsNullOrEmpty(priority))
            queryParams.Add($"priority={Uri.EscapeDataString(priority)}");
        
        if (assignedToUserId.HasValue)
            queryParams.Add($"assignedToUserId={assignedToUserId.Value}");
        
        var query = string.Join("&", queryParams);
        return await httpClient.GetFromJsonAsync<List<TaskModel>>($"/api/v1/tasks?{query}", cancellationToken)
            ?? new List<TaskModel>();
    }

    public async Task<TaskModel?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<TaskModel>($"/api/v1/tasks/{id}", cancellationToken);
    }

    public async Task<TaskModel?> CreateAsync(AddTaskRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/v1/tasks", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await response.Content.ReadFromJsonAsync<TaskModel>(cancellationToken: cancellationToken);
    }

    public async Task<bool> UpdateAsync(int id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync($"/api/v1/tasks/{id}", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync($"/api/v1/tasks/{id}", cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AssignAsync(int id, int assignedToUserId, CancellationToken cancellationToken = default)
    {
        var request = new { AssignedToUserId = assignedToUserId };
        var response = await httpClient.PatchAsJsonAsync($"/api/v1/tasks/{id}/assign", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<BoardResponse?> GetBoardAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<BoardResponse>($"/api/v1/tasks/board/{projectId}", cancellationToken);
    }

    public async Task<bool> ChangeStatusAsync(int id, string status, CancellationToken cancellationToken = default)
    {
        var request = new { Status = status };
        var response = await httpClient.PatchAsJsonAsync($"/api/v1/tasks/{id}/status", request, cancellationToken);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<TaskCommentModel>> GetCommentsAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<List<TaskCommentModel>>($"/api/v1/tasks/{taskId}/comments", cancellationToken)
            ?? new List<TaskCommentModel>();
    }

    public async Task<TaskCommentModel?> AddCommentAsync(int taskId, AddTaskCommentRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/v1/tasks/{taskId}/comments", request, cancellationToken);
        
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }
        
        return await response.Content.ReadFromJsonAsync<TaskCommentModel>(cancellationToken: cancellationToken);
    }
}
