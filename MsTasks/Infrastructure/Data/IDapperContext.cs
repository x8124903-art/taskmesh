using System.Data;

namespace MsTasks.Infrastructure.Data;

public interface IDapperContext
{
    IDbConnection CreateConnection();
    Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> ExecuteScalarAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
}
