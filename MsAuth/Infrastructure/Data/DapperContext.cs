
using System.Data;
using System.Diagnostics.CodeAnalysis;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Options;
using MsAuth.Infrastructure.Options;
using Dapper;

namespace MsAuth.Infrastructure.Data;

public interface IDapperContext
{
    IDbConnection CreateConnection();
    Task<T?> QueryFirstOrDefaultAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<T>> QueryAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<T> ExecuteScalarAsync<T>(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
    Task<int> ExecuteAsync(IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default);
}

[ExcludeFromCodeCoverage]
public sealed class DapperContext : IDapperContext
{
    private readonly string _connectionString;

    public DapperContext(IOptions<DatabaseOptions> options)
    {
        _connectionString = options.Value.DefaultConnection;
    }

    public IDbConnection CreateConnection()
    {
        return new MySqlConnection(_connectionString);
    }

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
