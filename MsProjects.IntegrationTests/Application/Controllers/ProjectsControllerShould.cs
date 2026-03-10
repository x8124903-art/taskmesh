using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using MySql.Data.MySqlClient;
using Dapper;

namespace MsProjects.IntegrationTests.Application.Controllers;

public class ProjectsControllerFixture : IDisposable
{
    private bool _isDisposed;
    
    public HttpClient Client { get; }

    public ProjectsControllerFixture()
    {
        InitializeTestDatabase();

        var webAppFactory = new WebApplicationFactory<Program>();

        var settingsFile = Path.Combine(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!,
            "appsettings.Test.json");

        Client = webAppFactory
            .WithWebHostBuilder(builder =>
            {
                builder
                    .UseEnvironment("Test")
                    .ConfigureAppConfiguration(c => c
                        .AddJsonFile(settingsFile))
                    .ConfigureTestServices(services => 
                    { 
                        services.AddSingleton<IDistributedCache>(new Mock<IDistributedCache>().Object);
                    });
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        Client.DefaultRequestHeaders.Add("X-User-Id", "1");
    }

    private void InitializeTestDatabase()
    {
        var connectionString = "Server=localhost;Port=3306;Uid=root;Pwd=TaskMesh2024!;";
        
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        connection.Execute("CREATE DATABASE IF NOT EXISTS taskmesh_projects_test;");
        connection.Execute("USE taskmesh_projects_test;");

        // Drop existing tables to recreate with correct schema
        connection.Execute("DROP TABLE IF EXISTS ProjectInvitation;");
        connection.Execute("DROP TABLE IF EXISTS ProjectMember;");
        connection.Execute("DROP TABLE IF EXISTS Project;");
        connection.Execute("DROP TABLE IF EXISTS ProjectStatus;");
        connection.Execute("DROP TABLE IF EXISTS ProjectRole;");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS ProjectRole (
                IdProjectRole INT PRIMARY KEY,
                Name VARCHAR(50) NOT NULL UNIQUE
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        connection.Execute(@"
            INSERT INTO ProjectRole (IdProjectRole, Name) VALUES 
                (1, 'Owner'), (2, 'Admin'), (3, 'Member'), (4, 'Viewer')
            ON DUPLICATE KEY UPDATE Name=Name;
        ");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS ProjectStatus (
                IdProjectStatus INT PRIMARY KEY,
                Name VARCHAR(50) NOT NULL UNIQUE,
                Description VARCHAR(200)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        connection.Execute(@"
            INSERT INTO ProjectStatus (IdProjectStatus, Name, Description) VALUES 
                (1, 'Active', 'Proyecto activo en desarrollo'),
                (2, 'Paused', 'Proyecto pausado temporalmente'),
                (3, 'Completed', 'Proyecto completado exitosamente'),
                (4, 'Archived', 'Proyecto archivado')
            ON DUPLICATE KEY UPDATE Name=Name;
        ");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Project (
                IdProject INT AUTO_INCREMENT PRIMARY KEY,
                Name VARCHAR(100) NOT NULL,
                Description VARCHAR(500),
                Status INT NOT NULL,
                CreatedBy INT NOT NULL,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                IsDeleted BOOLEAN DEFAULT FALSE,
                INDEX idx_created_by (CreatedBy),
                INDEX idx_status (Status),
                FOREIGN KEY (Status) REFERENCES ProjectStatus(IdProjectStatus)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS ProjectMember (
                IdProjectMember INT AUTO_INCREMENT PRIMARY KEY,
                ProjectId INT NOT NULL,
                UserId INT NOT NULL,
                Role INT NOT NULL,
                UserName VARCHAR(100) DEFAULT NULL,
                Email VARCHAR(255) DEFAULT NULL,
                InvitedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                JoinedAt DATETIME NULL,
                INDEX idx_project_id (ProjectId),
                INDEX idx_user_id (UserId),
                UNIQUE KEY unique_project_user (ProjectId, UserId),
                FOREIGN KEY (ProjectId) REFERENCES Project(IdProject) ON DELETE CASCADE,
                FOREIGN KEY (Role) REFERENCES ProjectRole(IdProjectRole)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS ProjectInvitation (
                IdProjectInvitation INT AUTO_INCREMENT PRIMARY KEY,
                ProjectId INT NOT NULL,
                Email VARCHAR(255) NOT NULL,
                Role INT NOT NULL,
                Token VARCHAR(500) NOT NULL UNIQUE,
                Status VARCHAR(20) NOT NULL,
                InvitedByUserId INT NOT NULL,
                InvitedByName VARCHAR(100) DEFAULT NULL,
                InvitedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                ExpiresAt DATETIME NOT NULL,
                AcceptedAt DATETIME NULL,
                RejectedAt DATETIME NULL,
                INDEX idx_token (Token),
                INDEX idx_project (ProjectId),
                INDEX idx_email (Email),
                INDEX idx_status (Status),
                INDEX idx_role (Role),
                FOREIGN KEY (ProjectId) REFERENCES Project(IdProject) ON DELETE CASCADE,
                FOREIGN KEY (Role) REFERENCES ProjectRole(IdProjectRole)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        // Clean and insert test data
        connection.Execute("DELETE FROM ProjectInvitation;");
        connection.Execute("DELETE FROM ProjectMember;");
        connection.Execute("DELETE FROM Project;");
        
        connection.Execute(@"
            INSERT INTO Project (Name, Description, Status, CreatedBy, CreatedAt) 
            VALUES ('Test Project 1', 'Integration test project', 1, 1, UTC_TIMESTAMP());
        ");

        connection.Execute(@"
            INSERT INTO ProjectMember (ProjectId, UserId, Role, UserName, Email, InvitedAt, JoinedAt) 
            VALUES (1, 1, 1, 'Test Owner', 'owner@test.com', UTC_TIMESTAMP(), UTC_TIMESTAMP());
        ");
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed) return;

        if (disposing)
        {
            Client.Dispose();
        }

        _isDisposed = true;
    }
}

public sealed class ProjectsControllerShould(ProjectsControllerFixture fixture)
    : IClassFixture<ProjectsControllerFixture>
{
    [Fact]
    public async Task GetAll_ReturnsOk_WithProjectList()
    {
        // Arrange & Act
        var response = await fixture.Client.GetAsync("projects");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("Project");
    }

    [Fact]
    public async Task Get_ReturnsOk_WhenProjectExists()
    {
        // Arrange - create a fresh project so the test doesn't depend on other tests
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
        var createResponse = await fixture.Client.PostAsync("projects", createContent);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var createResult = await createResponse.Content.ReadAsStringAsync();
        var created = JsonSerializer.Deserialize<JsonElement>(createResult);
        var projectId = created.GetProperty("idProject").GetInt32();

        // Act
        var response = await fixture.Client.GetAsync($"projects/{projectId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var stringResult = await response.Content.ReadAsStringAsync();
        stringResult.Should().Contain("Get Test Project");
    }

    [Fact]
    public async Task Add_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
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

        // Act
        var response = await fixture.Client.PostAsync("projects", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Update_ReturnsNoContent_WhenValidRequest()
    {
        // Arrange
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

        // Act
        var response = await fixture.Client.PutAsync("projects/1", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenProjectExists()
    {
        // Arrange
        const int projectId = 1;

        // Act
        var response = await fixture.Client.DeleteAsync($"projects/{projectId}");
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task Add_ReturnsBadRequest_WhenNameIsEmpty()
    {
        // Arrange
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

        // Act
        var response = await fixture.Client.PostAsync("projects", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
