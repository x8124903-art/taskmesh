namespace MsTasks.Application.Models;

public sealed record TaskCommentModel(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string Comment,
    DateTime CreatedAt
);
