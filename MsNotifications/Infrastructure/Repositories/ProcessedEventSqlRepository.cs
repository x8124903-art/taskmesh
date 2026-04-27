using Dapper;
using MsNotifications.Infrastructure.Data;

namespace MsNotifications.Infrastructure.Repositories;

public sealed class ProcessedEventSqlRepository : IProcessedEventRepository
{
    private readonly IDapperContext _context;

    public ProcessedEventSqlRepository(IDapperContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEventProcessedAsync(string eventId, CancellationToken cancellationToken = default)
    {
        var count = await _context.Connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                QueriesMySql.IsEventProcessed,
                new { EventId = eventId },
                cancellationToken: cancellationToken));

        return count > 0;
    }

    public async Task MarkEventAsProcessedAsync(string eventId, string eventType, CancellationToken cancellationToken = default)
    {
        await _context.Connection.ExecuteAsync(
            new CommandDefinition(
                QueriesMySql.InsertProcessedEvent,
                new
                {
                    EventId = eventId,
                    EventType = eventType,
                    ProcessedAt = DateTime.UtcNow
                },
                cancellationToken: cancellationToken));
    }
}
