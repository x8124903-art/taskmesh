namespace MsProjects.Domain.Services.Authorization;

/// <summary>
/// Service for checking user authorization and roles in projects.
/// </summary>
public interface IProjectAuthorizationService
{
    /// <summary>
    /// Checks if a user is a member of a project.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="projectId">The project ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is a member, false otherwise.</returns>
    Task<bool> IsProjectMemberAsync(int userId, int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has one of the specified roles in a project.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="projectId">The project ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <param name="allowedRoles">The roles to check for.</param>
    /// <returns>True if the user has one of the allowed roles, false otherwise.</returns>
    Task<bool> HasProjectRoleAsync(int userId, int projectId, CancellationToken cancellationToken, params string[] allowedRoles);

    /// <summary>
    /// Checks if a user is the owner of a project.
    /// </summary>
    /// <param name="userId">The user ID to check.</param>
    /// <param name="projectId">The project ID to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the user is the owner, false otherwise.</returns>
    Task<bool> IsProjectOwnerAsync(int userId, int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the role of a user in a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The role name, or null if the user is not a member.</returns>
    Task<string?> GetUserProjectRoleAsync(int userId, int projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all project IDs where a user is a member or owner.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of project IDs.</returns>
    Task<List<int>> GetUserProjectIdsAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cached list of projects for a specific user.
    /// </summary>
    /// <param name="userId">The user ID whose cache should be invalidated.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateUserProjectsCacheAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cached role for a specific user in a project.
    /// </summary>
    /// <param name="userId">The user ID.</param>
    /// <param name="projectId">The project ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateUserProjectRoleCacheAsync(int userId, int projectId, CancellationToken cancellationToken = default);
}
