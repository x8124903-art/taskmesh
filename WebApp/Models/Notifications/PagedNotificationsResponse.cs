namespace WebApp.Models.Notifications;

public sealed record PagedNotificationsResponse(
    IEnumerable<NotificationModel> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize);
