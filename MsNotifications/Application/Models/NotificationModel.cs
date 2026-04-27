namespace MsNotifications.Application.Models;

public sealed record NotificationModel(
    int IdNotification,
    int UserId,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType,
    int? RelatedEntityId,
    int? RelatedProjectId,
    bool IsRead,
    DateTime CreatedAt
);
