using MsAuth.Infrastructure.Data;
using MsAuth.Application.Models;

namespace MsAuth.IntegrationTests.Infrastructure.Data;

public class TestDapperContext : IDapperContext
{
    public System.Data.IDbConnection CreateConnection()
    {
        return new Mock<System.Data.IDbConnection>().Object;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        const string fixturesPath = "Fixtures/Application/Controllers/Auth/";
        
        if (query.Contains("SELECT") && query.Contains("User") && query.Contains("Email"))
        {
            var userResponse = await File.ReadAllTextAsync($"{fixturesPath}UserResponse.json", cancellationToken);
            var user = JsonSerializer.Deserialize<UserModel>(userResponse);
            return (T?)(object?)user;
        }
        
        return default;
    }

    public async Task<T> ExecuteScalarAsync<T>(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        if (query.Contains("INSERT"))
        {
            return (T)(object)1;
        }
        
        return default!;
    }

    public Task<int> ExecuteAsync(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(1);
    }

    public Task<IEnumerable<T>> QueryAsync<T>(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("QueryAsync not implemented in TestDapperContext");
    }
}
