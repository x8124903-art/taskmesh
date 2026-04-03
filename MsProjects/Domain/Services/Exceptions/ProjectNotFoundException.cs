namespace MsProjects.Domain.Services.Exceptions;

/// <summary>
/// Exception thrown when a project is not found or has been deleted.
/// </summary>
public sealed class ProjectNotFoundException : DomainException
{
    public ProjectNotFoundException(int projectId)
        : base($"Project with ID {projectId} was not found or has been deleted.")
    {
        ProjectId = projectId;
    }

    public int ProjectId { get; }
}
