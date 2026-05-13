using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record TaskCommentModel(
    int IdTaskComment,
    int TaskId,
    int UserId,
    string Comment,
    DateTime CreatedAt
);
