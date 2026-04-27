namespace WebApp.Models.Tasks;

public sealed record BoardResponse(
    Dictionary<string, List<TaskModel>> Columns
);
