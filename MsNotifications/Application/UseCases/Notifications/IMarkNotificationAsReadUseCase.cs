namespace MsNotifications.Application.UseCases.Notifications;

public interface IMarkNotificationAsReadUseCase
{
    Task ExecuteAsync(int idNotification, int userId, CancellationToken cancellationToken = default);
}
