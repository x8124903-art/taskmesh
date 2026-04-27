using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class MarkNotificationAsReadUseCase(INotificationService notificationService) : IMarkNotificationAsReadUseCase
{
    public async Task ExecuteAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        await notificationService.MarkAsReadAsync(idNotification, userId, cancellationToken);
    }
}
