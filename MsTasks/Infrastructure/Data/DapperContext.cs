using System.Data;
using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using Dapper;
using MsTasks.Infrastructure.Options;

namespace MsTasks.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public sealed class DapperContext : IDapperContext
{
    private readonly DatabaseOptions _options;
    
    public DapperContext(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }
    
    public IDbConnection CreateConnection() => new MySqlConnection(_options.DefaultConnection);

    public async Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
        return await connection.QueryFirstOrDefaultAsync<T>(command);
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
        return await connection.QueryAsync<T>(command);
    }

    public async Task<T> ExecuteScalarAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<T>(command);
    }

    public async Task<int> ExecuteAsync(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        var command = new CommandDefinition(query, parameters, cancellationToken: cancellationToken);
        return await connection.ExecuteAsync(command);
    }
}
