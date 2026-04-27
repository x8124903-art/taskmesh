namespace WebApp.Models.Tasks;

public sealed record TaskCommentModel(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string UserName,
    string Comment,
    DateTime CreatedAt
);
