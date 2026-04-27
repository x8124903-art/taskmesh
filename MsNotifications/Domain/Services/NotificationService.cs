using MsNotifications.Application.Models;
using MsNotifications.Domain.Services.Exceptions;
using MsNotifications.Infrastructure.Repositories;

namespace MsNotifications.Domain.Services;

public sealed class NotificationService(
    INotificationRepository notificationRepository,
    IProcessedEventRepository processedEventRepository) : INotificationService
{
    public async Task<List<NotificationModel>> GetByUserIdAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await notificationRepository.GetByUserIdAsync(userId, isRead, type, pageNumber, pageSize, cancellationToken);
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.GetUnreadCountByUserIdAsync(userId, cancellationToken);
    }

    public async Task MarkAsReadAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        var success = await notificationRepository.MarkAsReadAsync(idNotification, userId, cancellationToken);
        if (!success)
        {
            throw new NotificationNotFoundException(idNotification);
        }
    }

    public async Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.MarkAllAsReadAsync(userId, cancellationToken);
    }

    public async Task DeleteAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        var success = await notificationRepository.DeleteAsync(idNotification, userId, cancellationToken);
        if (!success)
        {
            throw new NotificationNotFoundException(idNotification);
        }
    }

    public async Task<int> CreateAsync(NotificationModel notification, CancellationToken cancellationToken = default)
    {
        return await notificationRepository.CreateAsync(notification, cancellationToken);
    }

    public async Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        return await processedEventRepository.IsEventProcessedAsync(eventId, cancellationToken);
    }

    public async Task MarkEventAsProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default)
    {
        await processedEventRepository.MarkEventAsProcessedAsync(eventId, eventType, cancellationToken);
    }
}
