using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Notifications;

[ExcludeFromCodeCoverage]
public sealed record NotificationModel(
    int IdNotification,
    int UserId,
    string Type,
    string Title,
    string Message,
    string RelatedEntityType,
    int RelatedEntityId,
    int? RelatedProjectId,
    bool IsRead,
    DateTime CreatedAt);
