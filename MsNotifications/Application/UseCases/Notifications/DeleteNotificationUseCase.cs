using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class DeleteNotificationUseCase(INotificationService notificationService) : IDeleteNotificationUseCase
{
    public async Task ExecuteAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        await notificationService.DeleteAsync(idNotification, userId, cancellationToken);
    }
}
