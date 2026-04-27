namespace MsNotifications.Application.Models;

public sealed record PagedNotificationsResponse(
    List<NotificationResponse> Notifications,
    int PageNumber,
    int PageSize,
    int TotalCount
);
