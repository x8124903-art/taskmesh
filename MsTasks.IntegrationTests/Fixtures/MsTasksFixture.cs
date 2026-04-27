using System.Net;
using Dapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Testcontainers.MySql;
using Testcontainers.Redis;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace MsTasks.IntegrationTests.Fixtures;

public sealed class MsTasksFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MySqlContainer _mySqlContainer = new MySqlBuilder()
        .WithImage("mysql:8.0")
        .WithDatabase("taskmesh_tasks_test")
        .WithUsername("root")
        .WithPassword("TaskMesh2024!")
        .WithPortBinding(3307, 3306)
        .Build();

    private readonly RedisContainer _redisContainer = new RedisBuilder()
        .WithImage("redis:7-alpine")
        .WithPortBinding(6380, 6379)
        .Build();

    private WireMockServer? _wireMockServer;

    public HttpClient HttpClient { get; private set; } = default!;
    public string ConnectionString { get; private set; } = string.Empty;
    public WireMockServer WireMockServer => _wireMockServer ?? throw new InvalidOperationException("WireMock server not initialized");

    public async Task InitializeAsync()
    {
        await _mySqlContainer.StartAsync();
        await _redisContainer.StartAsync();

        ConnectionString = _mySqlContainer.GetConnectionString();

        await InitializeDatabaseAsync();

        _wireMockServer = WireMockServer.Start(5001);
        ConfigureDefaultMsProjectsMocks();

        HttpClient = CreateClient();
    }

    public new async Task DisposeAsync()
    {
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();

        await _mySqlContainer.StopAsync();
        await _redisContainer.StopAsync();

        await _mySqlContainer.DisposeAsync();
        await _redisContainer.DisposeAsync();

        HttpClient?.Dispose();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.SetBasePath(AppContext.BaseDirectory);
            config.AddJsonFile("appsettings.Test.json", optional: false);
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Database:DefaultConnection"] = ConnectionString,
                ["RedisCache:ConnectionString"] = _redisContainer.GetConnectionString(),
                ["RedisCache:Enabled"] = "true",
                ["MsProjects:BaseUrl"] = _wireMockServer?.Urls[0] ?? "http://localhost:5001"
            });
        });

        builder.ConfigureTestServices(services =>
        {
        });
    }

    private async Task InitializeDatabaseAsync()
    {
        var scriptsPath = Path.Combine(AppContext.BaseDirectory, "Scripts");

        var schemaScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "create-tasks-schema.sql"));
        await ExecuteSqlScriptAsync(schemaScript);

        var dataScript = await File.ReadAllTextAsync(Path.Combine(scriptsPath, "insert-tasks-data.sql"));
        await ExecuteSqlScriptAsync(dataScript);
    }

    private async Task ExecuteSqlScriptAsync(string script)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        await connection.OpenAsync();

        var statements = script.Split(new[] { ";\r\n", ";\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var statement in statements)
        {
            var trimmedStatement = statement.Trim();
            if (!string.IsNullOrWhiteSpace(trimmedStatement))
            {
                await connection.ExecuteAsync(trimmedStatement);
            }
        }
    }

    private void ConfigureDefaultMsProjectsMocks()
    {
        if (_wireMockServer is null) return;

        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/10/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\": \"Owner\"}"));

        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/20/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\": \"Member\"}"));

        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/30/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\": \"Viewer\"}"));

        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/99/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(HttpStatusCode.NotFound));
    }

    public void ResetMsProjectsMocks()
    {
        _wireMockServer?.Reset();
        ConfigureDefaultMsProjectsMocks();
    }

    public async Task<int> CountTasksAsync()
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Task WHERE IsDeleted = 0");
    }

    public async Task<int> CountTaskCommentsAsync(int taskId)
    {
        await using var connection = new MySqlConnection(ConnectionString);
        return await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM TaskComment WHERE TaskId = @TaskId",
            new { TaskId = taskId });
    }
}
