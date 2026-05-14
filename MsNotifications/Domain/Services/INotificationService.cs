using MsNotifications.Application.Models;

namespace MsNotifications.Domain.Services;

public interface INotificationService
{
    Task<List<NotificationModel>> GetByUserIdAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task MarkAsReadAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task DeleteAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(NotificationModel notification, CancellationToken cancellationToken = default);
    Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default);
    Task MarkEventAsProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default);
}
