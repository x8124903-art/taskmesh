using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Testcontainers.MySql;
using Testcontainers.Redis;

namespace MsProjects.IntegrationTests.Application.Controllers;

public class ProjectsControllerFixture : WebApplicationFactory<Program>, IAsyncLifetime
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
                var eventBusMock = new Mock<MsProjects.Infrastructure.EventBus.IEventBus>();
                eventBusMock.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);
                services.AddSingleton(eventBusMock.Object);

                var authHttpClientMock = new Mock<MsProjects.Infrastructure.HttpClients.IAuthHttpClient>();
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

[CollectionDefinition(nameof(ProjectsControllerCollection))]
public class ProjectsControllerCollection : ICollectionFixture<ProjectsControllerFixture>;

[Collection(nameof(ProjectsControllerCollection))]
public sealed class ProjectsControllerShould(ProjectsControllerFixture fixture)
{
    private HttpClient CreateClient()
    {
        var client = fixture.CreateClient();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        return client;
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithProjectList()
    {
        var client = CreateClient();

        var response = await client.GetAsync("projects");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("Project");
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenProjectExists()
    {
        var client = CreateClient();

        var createRequest = new
        {
            name = "Get Test Project",
            description = "Project for Get test",
            status = 1
        };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            System.Text.Encoding.UTF8,
            "application/json");
        var createResponse = await client.PostAsync("projects", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createResult);
        var projectId = created.GetProperty("idProject").GetInt32();

        var response = await client.GetAsync($"projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("Get Test Project");
    }

    [Fact]
    public async Task Add_ReturnsCreated_WhenValidRequest()
    {
        var client = CreateClient();
        var request = new
        {
            name = "New Project",
            description = "A new test project",
            status = 1
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("projects", content);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenValidRequest()
    {
        var client = CreateClient();

        var createRequest = new { name = "Update Target", description = "Desc", status = 1 };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            System.Text.Encoding.UTF8,
            "application/json");
        var createResponse = await client.PostAsync("projects", createContent);
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createResult);
        var projectId = created.GetProperty("idProject").GetInt32();

        var request = new
        {
            name = "Updated Project",
            description = "Updated description",
            status = 1
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PutAsync($"projects/{projectId}", content);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProjectExists()
    {
        var client = CreateClient();

        var createRequest = new { name = "Delete Target", description = "Desc", status = 1 };
        var createContent = new StringContent(
            JsonSerializer.Serialize(createRequest),
            System.Text.Encoding.UTF8,
            "application/json");
        var createResponse = await client.PostAsync("projects", createContent);
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createResult);
        var projectId = created.GetProperty("idProject").GetInt32();

        var response = await client.DeleteAsync($"projects/{projectId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Add_ReturnsBadRequest_WhenNameIsEmpty()
    {
        var client = CreateClient();
        var request = new
        {
            name = "",
            description = "Test",
            status = 1
        };
        var content = new StringContent(
            JsonSerializer.Serialize(request),
            System.Text.Encoding.UTF8,
            "application/json");

        var response = await client.PostAsync("projects", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
