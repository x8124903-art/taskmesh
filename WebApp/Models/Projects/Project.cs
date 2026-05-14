using System.Diagnostics.CodeAnalysis;

namespace WebApp.Models.Projects;

[ExcludeFromCodeCoverage]
public class Project
{
    public int IdProject { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
