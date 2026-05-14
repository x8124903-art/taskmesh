using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Infrastructure.Data;

[ExcludeFromCodeCoverage]
internal static class QueriesMySql
{
    internal const string GetByUserId = @"
        SELECT 
            IdNotification,
            UserId,
            Type,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityId,
            RelatedProjectId,
            IsRead,
            CreatedAt
        FROM Notification
        WHERE UserId = @UserId
          AND (@IsRead IS NULL OR IsRead = @IsRead)
          AND (@Type IS NULL OR Type = @Type)
        ORDER BY CreatedAt DESC
        LIMIT @PageSize OFFSET @Offset";

    internal const string GetUnreadCountByUserId = @"
        SELECT COUNT(*)
        FROM Notification
        WHERE UserId = @UserId
          AND IsRead = FALSE";

    internal const string GetById = @"
        SELECT 
            IdNotification,
            UserId,
            Type,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityId,
            RelatedProjectId,
            IsRead,
            CreatedAt
        FROM Notification
        WHERE IdNotification = @IdNotification";

    internal const string MarkAsRead = @"
        UPDATE Notification
        SET IsRead = TRUE
        WHERE IdNotification = @IdNotification
          AND UserId = @UserId";

    internal const string MarkAllAsRead = @"
        UPDATE Notification
        SET IsRead = TRUE
        WHERE UserId = @UserId
          AND IsRead = FALSE";

    internal const string Delete = @"
        DELETE FROM Notification
        WHERE IdNotification = @IdNotification
          AND UserId = @UserId";

    internal const string Create = @"
        INSERT INTO Notification (
            UserId,
            Type,
            Title,
            Message,
            RelatedEntityType,
            RelatedEntityId,
            RelatedProjectId,
            IsRead,
            CreatedAt
        ) VALUES (
            @UserId,
            @Type,
            @Title,
            @Message,
            @RelatedEntityType,
            @RelatedEntityId,
            @RelatedProjectId,
            @IsRead,
            @CreatedAt
        );
        SELECT LAST_INSERT_ID();";

    internal const string IsEventProcessed = @"
        SELECT COUNT(*)
        FROM ProcessedEvent
        WHERE EventId = @EventId";

    internal const string InsertProcessedEvent = @"
        INSERT INTO ProcessedEvent (
            EventId,
            EventType,
            ProcessedAt
        ) VALUES (
            @EventId,
            @EventType,
            @ProcessedAt
        )";
}
