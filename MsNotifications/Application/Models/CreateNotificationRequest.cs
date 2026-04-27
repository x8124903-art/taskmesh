namespace MsNotifications.Application.Models;

public sealed record CreateNotificationRequest(
    int UserId,
    string Type,
    string Title,
    string Message,
    string? RelatedEntityType = null,
    int? RelatedEntityId = null,
    int? RelatedProjectId = null
);
