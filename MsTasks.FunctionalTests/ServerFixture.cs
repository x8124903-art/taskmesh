using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MsTasks.Infrastructure.EventBus;
using Testcontainers.MySql;
using Testcontainers.Redis;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace MsTasks.FunctionalTests;

public class ServerFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mySqlContainer;
    private RedisContainer? _redisContainer;
    private WireMockServer? _wireMockServer;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    
    public WireMockServer WireMockServer => _wireMockServer 
        ?? throw new InvalidOperationException("WireMockServer not initialized");
    
    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                await GenerateMySql();
                await GenerateRedis();
                GenerateWireMock();
                _initialized = true;
            }
        }
        finally
        {
            _initLock.Release();
        }
    }
    
    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_mySqlContainer is not null)
            await _mySqlContainer.DisposeAsync();
        if (_redisContainer is not null)
            await _redisContainer.DisposeAsync();
        _wireMockServer?.Stop();
        _wireMockServer?.Dispose();
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        InitializeAsync().GetAwaiter().GetResult();
        
        var settingsFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "appsettings.Test.json");
        var entries = new List<KeyValuePair<string, string?>>
        {
            new("Database:DefaultConnection", _mySqlContainer!.GetConnectionString()),
            new("RedisCache:ConnectionString", _redisContainer!.GetConnectionString()),
            new("MsProjects:BaseUrl", _wireMockServer!.Url!),
            new("EventBus:ConnectionString", "amqp://guest:guest@localhost:5672"),
            new("Observability:OtlpEndpoint", "")
        };
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration(c => c
                .AddJsonFile(settingsFile, true)
                .AddInMemoryCollection(entries!))
            .ConfigureTestServices(services =>
            {
                var eventBusMock = new Mock<IEventBus>();
                eventBusMock.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(eventBusMock.Object);
            });
    }
    
    private async Task GenerateMySql()
    {
        _mySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0.29")
            .WithDatabase("taskmesh_tasks")
            .Build();
        await _mySqlContainer.StartAsync();
        
        var createSchema = await File.ReadAllTextAsync("Scripts/create-tasks-schema.sql");
        await _mySqlContainer.ExecScriptAsync(createSchema);
        
        var insertData = await File.ReadAllTextAsync("Scripts/insert-tasks-data.sql");
        await _mySqlContainer.ExecScriptAsync(insertData);
    }
    
    private async Task GenerateRedis()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:6.2.6")
            .Build();
        
        await _redisContainer.StartAsync();
    }
    
    private void GenerateWireMock()
    {
        _wireMockServer = WireMockServer.Start();
        
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/10/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\":\"Owner\"}"));
        
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/20/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\":\"Member\"}"));
        
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/30/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"role\":\"Viewer\"}"));
        
        _wireMockServer
            .Given(Request.Create()
                .WithPath("/projects/1/members/99/role")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404));
    }
    
    public void ResetMsProjectsMocks()
    {
        _wireMockServer?.Reset();
        GenerateWireMock();
    }
}
