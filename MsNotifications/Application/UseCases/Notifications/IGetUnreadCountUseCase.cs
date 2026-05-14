using MsNotifications.Application.Models;

namespace MsNotifications.Application.UseCases.Notifications;

public interface IGetUnreadCountUseCase
{
    Task<UnreadCountResponse> ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
