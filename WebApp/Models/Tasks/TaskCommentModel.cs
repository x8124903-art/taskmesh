using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Tasks;

[ExcludeFromCodeCoverage]
public sealed record TaskCommentModel(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string UserName,
    string Comment,
    DateTime CreatedAt
);
