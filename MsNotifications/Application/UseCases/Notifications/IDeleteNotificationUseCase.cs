namespace MsNotifications.Application.UseCases.Notifications;

public interface IDeleteNotificationUseCase
{
    Task ExecuteAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
}
