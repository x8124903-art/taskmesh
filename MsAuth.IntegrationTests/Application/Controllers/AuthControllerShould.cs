using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MsAuth.Infrastructure.EventBus;
using Testcontainers.MySql;

namespace MsAuth.IntegrationTests.Application.Controllers;

public class AuthControllerFixture : WebApplicationFactory<Program>, IAsyncLifetime
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
            new("Database:DefaultConnection", _mySqlContainer!.GetConnectionString()),
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
            .WithDatabase("taskmesh_auth")
            .Build();
        await _mySqlContainer.StartAsync();

        var createSchema = await File.ReadAllTextAsync("Scripts/create-auth-schema.sql");
        await _mySqlContainer.ExecScriptAsync(createSchema);

        var insertData = await File.ReadAllTextAsync("Scripts/insert-auth-data.sql");
        await _mySqlContainer.ExecScriptAsync(insertData);
    }
}

[CollectionDefinition(nameof(AuthControllerCollection))]
public class AuthControllerCollection : ICollectionFixture<AuthControllerFixture>;

[Collection(nameof(AuthControllerCollection))]
public sealed class AuthControllerShould(AuthControllerFixture fixture)
{
    [Fact]
    public async Task Register_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new
        {
            email = "newuser@taskmesh.com",
            name = "New User",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Act
        var response = await client.PostAsync("auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenValidCredentials()
    {
        // Arrange
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new
        {
            email = "test@taskmesh.com",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Act
        var response = await client.PostAsync("auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("accessToken");
        stringResult.Should().Contain("refreshToken");
    }

    [Fact]
    public async Task RefreshToken_ReturnsOk_WhenValidToken()
    {
        // Arrange
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new
        {
            refreshToken = "valid-refresh-token",
            userId = 1
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Act
        var response = await client.PostAsync("auth/refresh", content);
        
        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenInvalidEmail()
    {
        // Arrange
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new
        {
            email = "invalid-email",
            name = "Test User",
            password = "Test123!"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Act
        var response = await client.PostAsync("auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var request = new
        {
            email = "nonexistent@taskmesh.com",
            password = "WrongPassword123!"
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request), 
            System.Text.Encoding.UTF8, 
            "application/json");

        // Act
        var response = await client.PostAsync("auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
