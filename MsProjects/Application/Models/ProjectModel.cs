namespace MsProjects.Application.Models
{
    public sealed record ProjectModel(
        int IdProject, 
        string Name, 
        string Description, 
        int Status,
        string StatusName,
        int CreatedBy, 
        string CreatedByName,
        bool IsDeleted, 
        DateTime CreatedAt);
}