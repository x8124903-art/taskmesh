using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class MarkAllNotificationsAsReadUseCase(INotificationService notificationService) : IMarkAllNotificationsAsReadUseCase
{
    public async Task<int> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await notificationService.MarkAllAsReadAsync(userId, cancellationToken);
    }
}
