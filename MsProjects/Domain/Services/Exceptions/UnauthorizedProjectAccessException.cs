namespace MsProjects.Domain.Services.Exceptions;

/// <summary>
/// Exception thrown when a user attempts to access or modify a project without permission.
/// </summary>
public sealed class UnauthorizedProjectAccessException : DomainException
{
    public UnauthorizedProjectAccessException(int userId, int projectId, string action)
        : base($"User {userId} is not authorized to {action} project {projectId}.")
    {
        UserId = userId;
        ProjectId = projectId;
        Action = action;
    }

    public int UserId { get; }
    public int ProjectId { get; }
    public string Action { get; }
}
