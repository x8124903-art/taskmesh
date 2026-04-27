using System.Diagnostics.CodeAnalysis;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;
using MsProjects.Infrastructure.Persistence;

namespace MsProjects.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ProjectMemberSqlRepository : IProjectMemberRepository
{
    private readonly MsProjects.Infrastructure.Data.IDapperContext _context;

    public ProjectMemberSqlRepository(MsProjects.Infrastructure.Data.IDapperContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProjectMemberModel>> GetMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entities = await _context.QueryAsync<ProjectMemberEntity>(
            connection,
            QueriesMySql.GetMembersByProjectId,
            new { ProjectId = projectId },
            cancellationToken);
        return entities.Select(ToModel);
    }

    public async Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entity = await _context.QueryFirstOrDefaultAsync<ProjectMemberEntity>(
            connection,
            QueriesMySql.GetMemberByProjectIdAndUserId,
            new { ProjectId = projectId, UserId = userId },
            cancellationToken);
        
        if (entity == null)
            throw new InvalidOperationException($"Member not found in project {projectId}");

        var roleId = ProjectRoles.GetIdFromName(role);
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.UpdateMemberRole,
            new { IdProjectMember = entity.IdProjectMember, Role = roleId },
            cancellationToken);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var entity = await _context.QueryFirstOrDefaultAsync<ProjectMemberEntity>(
            connection,
            QueriesMySql.GetMemberByProjectIdAndUserId,
            new { ProjectId = projectId, UserId = userId },
            cancellationToken);
        
        if (entity == null)
            throw new InvalidOperationException($"Member not found in project {projectId}");

        await _context.ExecuteAsync(
            connection,
            QueriesMySql.DeleteMember,
            new { IdProjectMember = entity.IdProjectMember },
            cancellationToken);
    }

    public async Task<string?> GetUserRoleInProjectAsync(int userId, int projectId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var roleId = await _context.QueryFirstOrDefaultAsync<int?>(
            connection,
            QueriesMySql.GetUserRoleInProject,
            new { UserId = userId, ProjectId = projectId },
            cancellationToken);
        
        return roleId.HasValue ? ProjectRoles.GetNameFromId(roleId.Value) : null;
    }

    public async Task<List<int>> GetProjectIdsByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var result = await _context.QueryAsync<int>(
            connection,
            QueriesMySql.GetProjectIdsByUserId,
            new { UserId = userId },
            cancellationToken);
        return result.ToList();
    }

    public async Task<bool> ExistsByProjectIdAndUserIdAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var count = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.ExistsByProjectIdAndUserId,
            new { ProjectId = projectId, UserId = userId },
            cancellationToken);
        return count > 0;
    }

    public async Task<bool> ExistsMemberByProjectIdAndEmailAsync(int projectId, string email, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var count = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.ExistsMemberByProjectIdAndEmail,
            new { ProjectId = projectId, Email = email },
            cancellationToken);
        return count > 0;
    }

    public async Task<int?> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        return await _context.QueryFirstOrDefaultAsync<int?>(
            connection,
            QueriesMySql.GetUserIdByEmail,
            new { Email = email },
            cancellationToken);
    }

    public async Task AddMemberAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default, string? userName = null, string? email = null)
    {
        using var connection = _context.CreateConnection();
        var roleId = ProjectRoles.GetIdFromName(role);
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.AddMember,
            new
            {
                ProjectId = projectId,
                UserId = userId,
                Role = roleId,
                UserName = userName,
                Email = email,
                InvitedAt = DateTime.UtcNow,
                JoinedAt = DateTime.UtcNow
            },
            cancellationToken);
    }

    private static ProjectMemberModel ToModel(ProjectMemberEntity e) =>
        new(e.IdProjectMember, e.ProjectId, e.UserId, e.UserName, e.Email, e.Role, e.RoleName, e.InvitedAt, e.JoinedAt);
}
