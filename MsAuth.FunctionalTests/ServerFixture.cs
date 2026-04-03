using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.MySql;

namespace MsAuth.FunctionalTests;

public class ServerFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private MySqlContainer? _mySqlContainer;
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
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        InitializeAsync().GetAwaiter().GetResult();
        
        var settingsFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "appsettings.Test.json");
        var entries = new List<KeyValuePair<string, string?>>
        {
            new("Database:DefaultConnection", _mySqlContainer!.GetConnectionString())
        };
        builder
            .UseEnvironment("Test")
            .ConfigureAppConfiguration(c => c
                .AddJsonFile(settingsFile, true)
                .AddInMemoryCollection(entries!));
    }
    
    private async Task GenerateMySql()
    {
        _mySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0.29")
            .WithDatabase("taskmesh_auth")
            .Build();
        await _mySqlContainer.StartAsync();
        
        var createSchema = await File.ReadAllTextAsync("Scripts/create-auth-schema.sql");
        await _mySqlContainer.ExecScriptAsync(createSchema);
        
        var insertData = await File.ReadAllTextAsync("Scripts/insert-auth-data.sql");
        await _mySqlContainer.ExecScriptAsync(insertData);
    }
}
