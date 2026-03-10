using System.Diagnostics.CodeAnalysis;
using MsProjects.Application.Models;
using MsProjects.Domain.Services;

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
        return await _context.QueryAsync<ProjectMemberModel>(
            connection,
            QueriesMySql.GetMembersByProjectId,
            new { ProjectId = projectId },
            cancellationToken);
    }

    public async Task ChangeRoleAsync(int projectId, int userId, string role, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var member = await _context.QueryFirstOrDefaultAsync<ProjectMemberModel>(
            connection,
            QueriesMySql.GetMemberByProjectIdAndUserId,
            new { ProjectId = projectId, UserId = userId },
            cancellationToken);
        
        if (member == null)
            throw new InvalidOperationException($"Member not found in project {projectId}");

        var roleId = ProjectRoles.GetIdFromName(role);
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.UpdateMemberRole,
            new { IdProjectMember = member.IdProjectMember, Role = roleId },
            cancellationToken);
    }

    public async Task RemoveMemberAsync(int projectId, int userId, CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        var member = await _context.QueryFirstOrDefaultAsync<ProjectMemberModel>(
            connection,
            QueriesMySql.GetMemberByProjectIdAndUserId,
            new { ProjectId = projectId, UserId = userId },
            cancellationToken);
        
        if (member == null)
            throw new InvalidOperationException($"Member not found in project {projectId}");

        await _context.ExecuteAsync(
            connection,
            QueriesMySql.DeleteMember,
            new { IdProjectMember = member.IdProjectMember },
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
}
