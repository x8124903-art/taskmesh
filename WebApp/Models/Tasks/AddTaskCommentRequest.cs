using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Tasks;

[ExcludeFromCodeCoverage]
public sealed record AddTaskCommentRequest(
    string Comment
);
