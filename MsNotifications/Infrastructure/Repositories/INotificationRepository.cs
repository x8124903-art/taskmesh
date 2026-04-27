using MsNotifications.Application.Models;

namespace MsNotifications.Infrastructure.Repositories;

public interface INotificationRepository
{
    Task<List<NotificationModel>> GetByUserIdAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<NotificationModel?> GetByIdAsync(int idNotification, CancellationToken cancellationToken = default);
    Task<bool> MarkAsReadAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
    Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
    Task<int> CreateAsync(NotificationModel notification, CancellationToken cancellationToken = default);
}
