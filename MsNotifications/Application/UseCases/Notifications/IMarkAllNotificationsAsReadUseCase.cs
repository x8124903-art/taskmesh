namespace MsNotifications.Application.UseCases.Notifications;

public interface IMarkAllNotificationsAsReadUseCase
{
    Task<int> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
