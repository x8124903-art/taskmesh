using MsNotifications.Application.Models;
using MsNotifications.Domain.Services;

namespace MsNotifications.Application.UseCases.Notifications;

public sealed class CreateNotificationUseCase(INotificationService notificationService) : ICreateNotificationUseCase
{
    public async Task<int> ExecuteAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        var notification = new NotificationModel(
            IdNotification: 0,
            UserId: request.UserId,
            Type: request.Type,
            Title: request.Title,
            Message: request.Message,
            RelatedEntityType: request.RelatedEntityType,
            RelatedEntityId: request.RelatedEntityId,
            RelatedProjectId: request.RelatedProjectId,
            IsRead: false,
            CreatedAt: DateTime.UtcNow
        );

        return await notificationService.CreateAsync(notification, cancellationToken);
    }
}
