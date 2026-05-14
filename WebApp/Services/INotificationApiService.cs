using WebApp.Models.Notifications;

namespace WebApp.Services;

public interface INotificationApiService
{
    Task<PagedNotificationsResponse?> GetNotificationsAsync(
        bool? isRead = null,
        string? type = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<UnreadCountResponse?> GetUnreadCountAsync(CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int idNotification, CancellationToken cancellationToken = default);
    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
    Task DeleteAsync(int idNotification, CancellationToken cancellationToken = default);
}
