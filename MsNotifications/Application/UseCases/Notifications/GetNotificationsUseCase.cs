using MsNotifications.Application.Models;
using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class GetNotificationsUseCase(INotificationService notificationService) : IGetNotificationsUseCase
{
    public async Task<PagedNotificationsResponse> ExecuteAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var notifications = await notificationService.GetByUserIdAsync(userId, isRead, type, pageNumber, pageSize, cancellationToken);

        var responses = notifications.Select(n => new NotificationResponse(
            IdNotification: n.IdNotification,
            Type: n.Type,
            Title: n.Title,
            Message: n.Message,
            RelatedEntityType: n.RelatedEntityType,
            RelatedEntityId: n.RelatedEntityId,
            RelatedProjectId: n.RelatedProjectId,
            IsRead: n.IsRead,
            CreatedAt: n.CreatedAt
        )).ToList();

        var totalCount = responses.Count;

        return new PagedNotificationsResponse(
            Notifications: responses,
            PageNumber: pageNumber,
            PageSize: pageSize,
            TotalCount: totalCount
        );
    }
}
