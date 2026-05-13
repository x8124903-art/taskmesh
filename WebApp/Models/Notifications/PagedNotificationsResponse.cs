using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Notifications;

[ExcludeFromCodeCoverage]
public sealed record PagedNotificationsResponse(
    IEnumerable<NotificationModel> Notifications,
    int TotalCount,
    int PageNumber,
    int PageSize);
