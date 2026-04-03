using MsProjects.Infrastructure.Data;

namespace MsProjects.IntegrationTests.Infrastructure.Data;

public class TestDapperContext : IDapperContext
{
    public System.Data.IDbConnection CreateConnection()
    {
        return new Mock<System.Data.IDbConnection>().Object;
    }

    public async Task<T?> QueryFirstOrDefaultAsync<T>(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        const string fixturesPath = "Fixtures/Application/Controllers/Projects/";
        
        if (query.Contains("SELECT") && query.Contains("Project"))
        {
            var projectResponse = await File.ReadAllTextAsync($"{fixturesPath}ProjectResponse.json", cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var obj = JsonSerializer.Deserialize<T>(projectResponse, options);
            return obj;
        }
        
        return default;
    }

    public async Task<IEnumerable<T>> QueryAsync<T>(System.Data.IDbConnection connection, string query, object? parameters = null, CancellationToken cancellationToken = default)
    {
        const string fixturesPath = "Fixtures/Application/Controllers/Projects/";
        
        if (query.Contains("SELECT") && query.Contains("Project"))
        {
            var projectListResponse = await File.ReadAllTextAsync($"{fixturesPath}ProjectListResponse.json", cancellationToken);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var list = JsonSerializer.Deserialize<List<T>>(projectListResponse, options);
            return list ?? new List<T>();
        }
        
        return new List<T>();
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
}
