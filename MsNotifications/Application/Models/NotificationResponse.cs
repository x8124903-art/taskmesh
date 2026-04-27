namespace MsNotifications.Application.Models;

public sealed record NotificationResponse(
    int IdNotification,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType,
    int? RelatedEntityId,
    int? RelatedProjectId,
    bool IsRead,
    DateTime CreatedAt
);
