namespace MsProjects.Domain.Services.Exceptions;

/// <summary>
/// Exception thrown when an invalid role is specified.
/// </summary>
public sealed class InvalidRoleException : DomainException
{
    public InvalidRoleException(string role)
        : base($"Role '{role}' is not valid. Valid roles are: Owner, Admin, Member, Viewer.")
    {
        Role = role;
    }

    public string Role { get; }
}
