using Microsoft.Extensions.Options;
using MsNotifications.Infrastructure.Data;
using MsNotifications.Infrastructure.Options;
using Testcontainers.MySql;

namespace MsNotifications.IntegrationTests.Fixtures;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private MySqlContainer? _mySqlContainer;

    public IDapperContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        _mySqlContainer = new MySqlBuilder()
            .WithImage("mysql:8.0.29")
            .WithDatabase("taskmesh_notifications_test")
            .Build();
        await _mySqlContainer.StartAsync();

        var createSchema = await File.ReadAllTextAsync("Scripts/init-test-db.sql");
        await _mySqlContainer.ExecScriptAsync(createSchema);

        var options = Options.Create(new DatabaseOptions
        {
            Type = "MySql",
            DefaultConnection = _mySqlContainer.GetConnectionString()
        });

        Context = new DapperContext(options);
    }

    public void Cleanup()
    {
        if (_mySqlContainer is not null)
        {
            _mySqlContainer.ExecScriptAsync("DELETE FROM ProcessedEvent; DELETE FROM Notification; ALTER TABLE Notification AUTO_INCREMENT = 1;")
                .GetAwaiter().GetResult();
        }
    }

    public async Task DisposeAsync()
    {
        if (_mySqlContainer is not null)
            await _mySqlContainer.DisposeAsync();
    }
}
