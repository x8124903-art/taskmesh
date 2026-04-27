using MsNotifications.Application.Models;

namespace MsNotifications.Application.UseCases.Notifications;

public interface ICreateNotificationUseCase
{
    Task<int> ExecuteAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);
}
