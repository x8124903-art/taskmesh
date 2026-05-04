namespace WebApp.Models.Tasks;

public class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int? AssignedToUserId { get; set; }
    public string Priority { get; set; } = string.Empty;
    public DateTime? DueDate { get; set; }
    public int RowVersion { get; set; }
}
