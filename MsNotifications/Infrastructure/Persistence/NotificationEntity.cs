namespace MsNotifications.Infrastructure.Persistence;

internal sealed record NotificationEntity(
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
