using System.Diagnostics.CodeAnalysis;

namespace MsTasks.Application.Models;

[ExcludeFromCodeCoverage]
public sealed record BoardResponse(Dictionary<string, List<TaskModel>> Columns);
