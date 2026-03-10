namespace MsProjects.Domain.Services;

/// <summary>
/// Defines the available roles for project members.
/// Used across the application to ensure consistent role naming and IDs.
/// </summary>
public static class ProjectRoles
{
    public const int OwnerId = 1;
    public const int AdminId = 2;
    public const int MemberId = 3;
    public const int ViewerId = 4;

    /// <summary>
    /// Project owner with full control including deletion and ownership transfer.
    /// </summary>
    public const string Owner = "Owner";

    /// <summary>
    /// Project administrator with full permissions except project deletion.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// Regular project member with read and write permissions.
    /// </summary>
    public const string Member = "Member";

    /// <summary>
    /// Project viewer with read-only access.
    /// </summary>
    public const string Viewer = "Viewer";

    /// <summary>
    /// Gets the role ID from role name.
    /// </summary>
    public static int GetIdFromName(string name) => name switch
    {
        Owner => OwnerId,
        Admin => AdminId,
        Member => MemberId,
        Viewer => ViewerId,
        _ => throw new ArgumentException($"Invalid role name: {name}", nameof(name))
    };

    /// <summary>
    /// Gets the role name from role ID.
    /// </summary>
    public static string GetNameFromId(int id) => id switch
    {
        OwnerId => Owner,
        AdminId => Admin,
        MemberId => Member,
        ViewerId => Viewer,
        _ => throw new ArgumentException($"Invalid role ID: {id}", nameof(id))
    };

    /// <summary>
    /// Gets all valid role names.
    /// </summary>
    public static readonly string[] AllRoles = { Owner, Admin, Member, Viewer };

    /// <summary>
    /// Validates if a role name is valid.
    /// </summary>
    /// <param name="role">The role name to validate.</param>
    /// <returns>True if the role is valid, false otherwise.</returns>
    public static bool IsValidRole(string role)
    {
        return role == Owner || role == Admin || role == Member || role == Viewer;
    }

    /// <summary>
    /// Gets roles that can manage members (invite, remove, change roles).
    /// </summary>
    public static readonly string[] ManagementRoles = { Owner, Admin };

    /// <summary>
    /// Gets roles that can edit project details.
    /// </summary>
    public static readonly string[] EditRoles = { Owner, Admin };
}
