using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using MsNotifications.Infrastructure.Options;
using System.Data;

namespace MsNotifications.Infrastructure.Data;

public sealed class DapperContext : IDapperContext
{
    private readonly DatabaseOptions _options;
    private IDbConnection? _connection;

    public DapperContext(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public IDbConnection Connection
    {
        get
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection?.Dispose();
                _connection = new MySqlConnection(_options.DefaultConnection);
                _connection.Open();
            }
            return _connection;
        }
    }

    public void Dispose()
    {
        if (_connection != null)
        {
            if (_connection.State == ConnectionState.Open)
                _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
    }
}
