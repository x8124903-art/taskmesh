using System.Diagnostics.CodeAnalysis;
using MsProjects.Application.Models;
using MsProjects.Infrastructure.Data;
using MsProjects.Domain.Services;
using MsProjects.Infrastructure.Persistence;

namespace MsProjects.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class ProjectInvitationSqlRepository : IProjectInvitationRepository
{
    private readonly IDapperContext _context;

    public ProjectInvitationSqlRepository(IDapperContext context)
    {
        _context = context;
    }

    public async Task<ProjectInvitationModel> CreateAsync(
        int projectId,
        string email,
        string role,
        string token,
        int invitedByUserId,
        string? invitedByName,
        DateTime expiresAt,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var createdAt = DateTime.UtcNow;
        var roleId = ProjectRoles.GetIdFromName(role);
        
        var invitationId = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.InsertInvitation,
            new
            {
                ProjectId = projectId,
                Email = email,
                Role = roleId,
                Token = token,
                InvitedBy = invitedByUserId,
                InvitedByName = invitedByName,
                CreatedAt = createdAt,
                ExpiresAt = expiresAt
            },
            cancellationToken);
        
        return (await GetByIdAsync(invitationId, cancellationToken))!;
    }

    public async Task<ProjectInvitationModel?> GetByIdAsync(
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var entity = await _context.QueryFirstOrDefaultAsync<ProjectInvitationEntity>(
            connection,
            QueriesMySql.GetInvitationById,
            new { InvitationId = invitationId },
            cancellationToken);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<ProjectInvitationModel?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var entity = await _context.QueryFirstOrDefaultAsync<ProjectInvitationEntity>(
            connection,
            QueriesMySql.GetInvitationByToken,
            new { Token = token },
            cancellationToken);
        return entity is null ? null : ToModel(entity);
    }

    public async Task<IEnumerable<ProjectInvitationModel>> GetPendingByProjectIdAsync(
        int projectId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var entities = await _context.QueryAsync<ProjectInvitationEntity>(
            connection,
            QueriesMySql.GetPendingInvitationsByProjectId,
            new { ProjectId = projectId },
            cancellationToken);
        return entities.Select(ToModel);
    }

    public async Task<IEnumerable<ProjectInvitationModel>> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var entities = await _context.QueryAsync<ProjectInvitationEntity>(
            connection,
            QueriesMySql.GetInvitationsByEmail,
            new { Email = email },
            cancellationToken);
        return entities.Select(ToModel);
    }

    public async Task AcceptAsync(
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.AcceptInvitation,
            new 
            { 
                InvitationId = invitationId,
                AcceptedAt = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task RejectAsync(
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.RejectInvitation,
            new 
            { 
                InvitationId = invitationId,
                RejectedAt = DateTime.UtcNow
            },
            cancellationToken);
    }

    public async Task DeleteAsync(
        int invitationId,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.DeleteInvitation,
            new { InvitationId = invitationId },
            cancellationToken);
    }

    public async Task MarkExpiredInvitationsAsync(CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        await _context.ExecuteAsync(
            connection,
            QueriesMySql.MarkExpiredInvitations,
            new { Now = DateTime.UtcNow },
            cancellationToken);
    }

    public async Task<bool> ExistsPendingByProjectAndEmailAsync(
        int projectId,
        string email,
        CancellationToken cancellationToken = default)
    {
        using var connection = _context.CreateConnection();
        
        var count = await _context.ExecuteScalarAsync<int>(
            connection,
            QueriesMySql.ExistsPendingByProjectAndEmail,
            new 
            { 
                ProjectId = projectId,
                Email = email
            },
            cancellationToken);
        
        return count > 0;
    }

    private static ProjectInvitationModel ToModel(ProjectInvitationEntity e) =>
        new(e.IdProjectInvitation, e.ProjectId, e.ProjectName, e.Email, e.Role, e.RoleName,
            e.Token, e.Status, e.InvitedBy, e.InvitedByName, e.CreatedAt, e.ExpiresAt, e.AcceptedAt, e.RejectedAt);
}
