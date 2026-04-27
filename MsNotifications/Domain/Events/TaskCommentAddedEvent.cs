namespace MsTasks.Domain.Events;

public sealed record TaskCommentAddedEvent(
    string EventId,
    DateTime OccurredAt,
    int TaskId,
    int ProjectId,
    int CommentId,
    int AuthorUserId,
    string TaskTitle,
    int TaskCreatedByUserId,
    int? TaskAssignedToUserId
);
