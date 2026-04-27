namespace MsTasks.Application.Models;

public sealed record BoardResponse(Dictionary<string, List<TaskModel>> Columns);
