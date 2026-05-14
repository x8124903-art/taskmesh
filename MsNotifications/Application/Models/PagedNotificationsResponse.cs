using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record PagedNotificationsResponse(
    List<NotificationResponse> Notifications,
    int PageNumber,
    int PageSize,
    int TotalCount
);
