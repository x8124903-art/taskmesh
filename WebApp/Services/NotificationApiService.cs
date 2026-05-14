using System.Net.Http.Json;
using WebApp.Models.Notifications;

namespace WebApp.Services;

public sealed class NotificationApiService(HttpClient httpClient) : INotificationApiService
{
    private const string BaseUrl = "/api/v1/notifications";

    public async Task<PagedNotificationsResponse?> GetNotificationsAsync(
        bool? isRead = null,
        string? type = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new List<string>();

        if (isRead.HasValue)
            queryParams.Add($"isRead={isRead.Value.ToString().ToLower()}");

        if (!string.IsNullOrWhiteSpace(type))
            queryParams.Add($"type={Uri.EscapeDataString(type)}");

        queryParams.Add($"pageNumber={page}");
        queryParams.Add($"pageSize={pageSize}");

        var query = string.Join("&", queryParams);
        var url = $"{BaseUrl}?{query}";

        return await httpClient.GetFromJsonAsync<PagedNotificationsResponse>(url, cancellationToken);
    }

    public async Task<UnreadCountResponse?> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        return await httpClient.GetFromJsonAsync<UnreadCountResponse>(
            $"{BaseUrl}/unread-count",
            cancellationToken);
    }

    public async Task MarkAsReadAsync(int idNotification, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsync(
            $"{BaseUrl}/{idNotification}/read",
            null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PatchAsync(
            $"{BaseUrl}/mark-all-read",
            null,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int idNotification, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync(
            $"{BaseUrl}/{idNotification}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
