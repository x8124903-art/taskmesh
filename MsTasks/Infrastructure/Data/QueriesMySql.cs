using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Infrastructure.Data;

[ExcludeFromCodeCoverage]
internal static class QueriesMySql
{
    internal const string GetByProjectId = @"
        SELECT 
            IdTask, Title, Description, ProjectId, AssignedToUserId, 
            Priority, Status, DueDate, CreatedBy, RowVersion, 
            CreatedAt, UpdatedAt, IsDeleted, DeletedAt
        FROM Task
        WHERE ProjectId = @ProjectId 
          AND IsDeleted = FALSE
          AND (@Status IS NULL OR Status = @Status)
          AND (@Priority IS NULL OR Priority = @Priority)
          AND (@AssignedToUserId IS NULL OR AssignedToUserId = @AssignedToUserId)
        ORDER BY CreatedAt DESC";

    internal const string GetById = @"
        SELECT 
            IdTask, Title, Description, ProjectId, AssignedToUserId, 
            Priority, Status, DueDate, CreatedBy, RowVersion, 
            CreatedAt, UpdatedAt, IsDeleted, DeletedAt
        FROM Task
        WHERE IdTask = @IdTask AND IsDeleted = FALSE";

    internal const string Create = @"
        INSERT INTO Task (Title, Description, ProjectId, AssignedToUserId, Priority, Status, DueDate, CreatedBy, RowVersion)
        VALUES (@Title, @Description, @ProjectId, @AssignedToUserId, @Priority, @Status, @DueDate, @CreatedBy, 1);
        SELECT LAST_INSERT_ID();";

    internal const string Update = @"
        UPDATE Task
        SET Title = @Title,
            Description = @Description,
            AssignedToUserId = @AssignedToUserId,
            Priority = @Priority,
            DueDate = @DueDate,
            RowVersion = RowVersion + 1,
            UpdatedAt = NOW()
        WHERE IdTask = @IdTask 
          AND RowVersion = @RowVersion 
          AND IsDeleted = FALSE";

    internal const string UpdateStatus = @"
        UPDATE Task
        SET Status = @Status,
            UpdatedAt = NOW()
        WHERE IdTask = @IdTask AND IsDeleted = FALSE";

    internal const string UpdateAssignment = @"
        UPDATE Task
        SET AssignedToUserId = @AssignedToUserId,
            UpdatedAt = NOW()
        WHERE IdTask = @IdTask AND IsDeleted = FALSE";

    internal const string SoftDelete = @"
        UPDATE Task
        SET IsDeleted = TRUE,
            DeletedAt = NOW()
        WHERE IdTask = @IdTask AND IsDeleted = FALSE";

    internal const string GetBoardByProjectId = @"
        SELECT 
            IdTask, Title, Description, ProjectId, AssignedToUserId, 
            Priority, Status, DueDate, CreatedBy, RowVersion, 
            CreatedAt, UpdatedAt, IsDeleted, DeletedAt
        FROM Task
        WHERE ProjectId = @ProjectId AND IsDeleted = FALSE
        ORDER BY Status, CreatedAt DESC";

    internal const string CreateComment = @"
        INSERT INTO TaskComment (TaskId, UserId, Comment)
        VALUES (@TaskId, @UserId, @Comment);
        SELECT LAST_INSERT_ID();";

    internal const string GetCommentsByTaskId = @"
        SELECT 
            IdTaskComment, TaskId, UserId, Comment, CreatedAt
        FROM TaskComment
        WHERE TaskId = @TaskId
        ORDER BY CreatedAt DESC";
}
