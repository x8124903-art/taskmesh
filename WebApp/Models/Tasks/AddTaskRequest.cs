namespace WebApp.Models.Tasks;

public class AddTaskRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int ProjectId { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? Priority { get; set; }
    public DateTime? DueDate { get; set; }
}
