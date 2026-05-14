using MsNotifications.Application.Models;
using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class GetUnreadCountUseCase(INotificationService notificationService) : IGetUnreadCountUseCase
{
    public async Task<UnreadCountResponse> ExecuteAsync(int userId, CancellationToken cancellationToken = default)
    {
        var count = await notificationService.GetUnreadCountByUserIdAsync(userId, cancellationToken);
        return new UnreadCountResponse(count);
    }
}
