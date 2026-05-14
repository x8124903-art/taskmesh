using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Tasks;

[ExcludeFromCodeCoverage]
public sealed record BoardResponse(
    Dictionary<string, List<TaskModel>> Columns
);
