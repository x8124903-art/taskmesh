using System.ComponentModel.DataAnnotations;

namespace MsProjects.Application.Models
{
    public sealed record AddProjectRequest(
        [Required(ErrorMessage = "Project name is required")]
        [MinLength(1, ErrorMessage = "Project name cannot be empty")]
        [MaxLength(100, ErrorMessage = "Project name cannot exceed 100 characters")]
        string Name, 
        
        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        string Description);
}