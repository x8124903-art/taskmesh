using Dapper;
using MsNotifications.Application.Models;
using MsNotifications.Infrastructure.Data;
using MsNotifications.Infrastructure.Persistence;
using System.Diagnostics.CodeAnalysis;

namespace MsNotifications.Infrastructure.Repositories;

[ExcludeFromCodeCoverage]
public sealed class NotificationSqlRepository : INotificationRepository
{
    private readonly IDapperContext _context;

    public NotificationSqlRepository(IDapperContext context)
    {
        _context = context;
    }

    public async Task<List<NotificationModel>> GetByUserIdAsync(
        int userId,
        bool? isRead,
        string? type,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var offset = (pageNumber - 1) * pageSize;
        var entities = await _context.Connection.QueryAsync<NotificationEntity>(
            new CommandDefinition(
                QueriesMySql.GetByUserId,
                new { UserId = userId, IsRead = isRead, Type = type, PageSize = pageSize, Offset = offset },
                cancellationToken: cancellationToken));

        return entities.Select(ToModel).ToList();
    }

    public async Task<int> GetUnreadCountByUserIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                QueriesMySql.GetUnreadCountByUserId,
                new { UserId = userId },
                cancellationToken: cancellationToken));
    }

    public async Task<NotificationModel?> GetByIdAsync(int idNotification, CancellationToken cancellationToken = default)
    {
        var entity = await _context.Connection.QueryFirstOrDefaultAsync<NotificationEntity>(
            new CommandDefinition(
                QueriesMySql.GetById,
                new { IdNotification = idNotification },
                cancellationToken: cancellationToken));

        return entity != null ? ToModel(entity) : null;
    }

    public async Task<bool> MarkAsReadAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _context.Connection.ExecuteAsync(
            new CommandDefinition(
                QueriesMySql.MarkAsRead,
                new { IdNotification = idNotification, UserId = userId },
                cancellationToken: cancellationToken));

        return rowsAffected > 0;
    }

    public async Task<int> MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
    {
        return await _context.Connection.ExecuteAsync(
            new CommandDefinition(
                QueriesMySql.MarkAllAsRead,
                new { UserId = userId },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(int idNotification, int userId, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _context.Connection.ExecuteAsync(
            new CommandDefinition(
                QueriesMySql.Delete,
                new { IdNotification = idNotification, UserId = userId },
                cancellationToken: cancellationToken));

        return rowsAffected > 0;
    }

    public async Task<int> CreateAsync(NotificationModel notification, CancellationToken cancellationToken = default)
    {
        return await _context.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                QueriesMySql.Create,
                new
                {
                    notification.UserId,
                    notification.Type,
                    notification.Title,
                    notification.Message,
                    notification.RelatedEntityType,
                    notification.RelatedEntityId,
                    notification.RelatedProjectId,
                    notification.IsRead,
                    notification.CreatedAt
                },
                cancellationToken: cancellationToken));
    }

    private static NotificationModel ToModel(NotificationEntity entity) => new(
        IdNotification: entity.IdNotification,
        UserId: entity.UserId,
        Type: entity.Type,
        Title: entity.Title,
        Message: entity.Message,
        RelatedEntityType: entity.RelatedEntityType,
        RelatedEntityId: entity.RelatedEntityId,
        RelatedProjectId: entity.RelatedProjectId,
        IsRead: entity.IsRead,
        CreatedAt: entity.CreatedAt
    );
}
