namespace MsProjects.Domain.Services.Exceptions;

/// <summary>
/// Exception thrown when attempting to add a user who is already a member of the project.
/// </summary>
public sealed class DuplicateMemberException : DomainException
{
    public DuplicateMemberException(int projectId, int userId)
        : base($"User {userId} is already a member of project {projectId}.")
    {
        UserId = userId;
        ProjectId = projectId;
    }

    public DuplicateMemberException(int projectId, string email)
        : base($"A user with email '{email}' is already a member of project {projectId}.")
    {
        Email = email;
        ProjectId = projectId;
    }

    public int? UserId { get; }
    public string? Email { get; }
    public int ProjectId { get; }
}
