using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MsProjects.Infrastructure.EventBus;
using MsProjects.Infrastructure.HttpClients;
using Testcontainers.MySql;
using Testcontainers.Redis;

namespace MsProjects.FunctionalTests;

public class ServerFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mySqlContainer;
    private RedisContainer? _redisContainer;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private bool _initialized;
    
    public async Task InitializeAsync()
    {
        await _initLock.WaitAsync();
        try
        {
            if (!_initialized)
            {
                await GenerateMySql();
                await GenerateRedis();
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
            new("EventBus:ConnectionString", "amqp://guest:guest@localhost:5672"),
            new("MsAuth:BaseUrl", "http://localhost:44310"),
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

                var authHttpClientMock = new Mock<IAuthHttpClient>();
                authHttpClientMock.Setup(x => x.GetUserIdByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((int?)null);
                services.AddSingleton(authHttpClientMock.Object);
            });
    }
    
    private async Task GenerateMySql()
    {
        _mySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0.29")
            .WithDatabase("taskmesh_projects")
            .Build();
        await _mySqlContainer.StartAsync();
        
        var createSchema = await File.ReadAllTextAsync("Scripts/create-projects-schema.sql");
        await _mySqlContainer.ExecScriptAsync(createSchema);
        
        var insertData = await File.ReadAllTextAsync("Scripts/insert-projects-data.sql");
        await _mySqlContainer.ExecScriptAsync(insertData);
    }
    
    private async Task GenerateRedis()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:6.2.6")
            .Build();
        
        await _redisContainer.StartAsync();
    }
}
