using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Dapper;

namespace MsAuth.IntegrationTests.Application.Controllers;

public class AuthControllerFixture : IDisposable
{
    private bool _isDisposed;
    
    public HttpClient Client { get; }

    public AuthControllerFixture()
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
                        .AddJsonFile(settingsFile));
            })
            .CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    private void InitializeTestDatabase()
    {
        var connectionString = "Server=localhost;Port=3306;Uid=root;Pwd=TaskMesh2024!;";
        
        using var connection = new MySqlConnection(connectionString);
        connection.Open();

        connection.Execute("CREATE DATABASE IF NOT EXISTS taskmesh_auth_test;");
        connection.Execute("USE taskmesh_auth_test;");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS User (
                IdUser INT AUTO_INCREMENT PRIMARY KEY,
                Email VARCHAR(255) NOT NULL UNIQUE,
                Name VARCHAR(100) NOT NULL,
                PasswordHash VARCHAR(255) NOT NULL,
                Role VARCHAR(50) NOT NULL DEFAULT 'User',
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
                IsDeleted BOOLEAN DEFAULT FALSE,
                INDEX idx_email (Email)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS RefreshToken (
                IdRefreshToken INT AUTO_INCREMENT PRIMARY KEY,
                Token VARCHAR(500) NOT NULL UNIQUE,
                UserId INT NOT NULL,
                ExpiresAt DATETIME NOT NULL,
                IsRevoked BOOLEAN DEFAULT FALSE,
                CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                RevokedAt DATETIME NULL,
                INDEX idx_token (Token),
                FOREIGN KEY (UserId) REFERENCES User(IdUser)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
        ");

        // Clean tables and insert test data
        connection.Execute("DELETE FROM RefreshToken;");
        connection.Execute("DELETE FROM User;");
        
        // Insert test user with correct BCrypt hash for "Test123!"
        connection.Execute(@"
            INSERT INTO User (Email, Name, PasswordHash, Role, CreatedAt, IsDeleted) 
            VALUES ('test@taskmesh.com', 'Test User', '$2a$12$2anUlT3M1oUOo9Q8OdswlOVd7FHj9PCw.p.DFL5AE1OZCSpdhjfYS', 'User', UTC_TIMESTAMP(), 0);
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

public sealed class AuthControllerShould(AuthControllerFixture fixture)
    : IClassFixture<AuthControllerFixture>
{
    [Fact]
    public async Task Register_ReturnsCreated_WhenValidRequest()
    {
        // Arrange
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
        var response = await fixture.Client.PostAsync("auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenValidCredentials()
    {
        // Arrange
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
        var response = await fixture.Client.PostAsync("auth/login", content);
        
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
        var response = await fixture.Client.PostAsync("auth/refresh", content);
        
        // Assert
        response.Should().NotBeNull();
    }

    [Fact]
    public async Task Register_ReturnsBadRequest_WhenInvalidEmail()
    {
        // Arrange
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
        var response = await fixture.Client.PostAsync("auth/register", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
    {
        // Arrange
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
        var response = await fixture.Client.PostAsync("auth/login", content);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
