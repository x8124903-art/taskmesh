using MsNotifications.Application.Models;

namespace MsNotifications.Application.UseCases.Notifications;

public interface IGetNotificationsUseCase
{
    Task<PagedNotificationsResponse> ExecuteAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
